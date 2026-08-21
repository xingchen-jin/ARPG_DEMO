using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ClimbType : int
{
    None = 0,
    LowClimb = 1,
    HighClimb = 2,
    Vault = 3,
}

public class ClimbDetector : MonoBehaviour
{
    private CharacterController controller;
    [SerializeField] private LayerMask climbableLayer;
    [Header("检测参数")]
    [SerializeField] private float forwardCheckDistance = 0.5f; //向前检测距离
    [SerializeField] private float vertiicalStep = 0.2f; //垂直检测步长
    [SerializeField] private float startHeight = 0.1f; //起始高度
    [SerializeField] private float endHeightMult = 1.2f; //结束高度倍数,乘以角色高度
    [SerializeField] private float lowClimbHeightMult = 0.6f; //低攀爬高度倍数，高于这个高度为高攀爬
    [SerializeField] private float minThroughHeight = 0.5f; //最小通过高度

    [SerializeField] private float minClimbHeightMult = 0.1f; //最小攀爬高度倍数

    [SerializeField]private float maxVaultWidth = 0.2f; //最大翻越宽度

    [Header("偏移量")]
    [SerializeField] private float forwardOffsetBase = 0f; //向前偏移的基准值，避免检测到自己

    void Awake()
    {
        if (controller == null)
        {
            controller = GetComponent<CharacterController>();
        }
    }
    /// <summary>
    /// 检测是否可以攀爬
    /// </summary>
    /// <param name="climbType">返回攀爬类型</param>
    /// <param name="wallPoint">墙面顶点</param>
    /// <param name="wallNormal">墙面法线</param>
    /// <returns></returns>
    public bool TryGetClimbInfo(out ClimbType climbType, out Vector3 wallPoint,out Vector3 wallNormal)
    {
        climbType = ClimbType.None;
        wallPoint = Vector3.zero;
        wallNormal = Vector3.zero;
        if (controller == null)
        {
            Debug.LogError("不存在角色控制器");
            return false;
        }
        Vector3 forward = transform.forward;
        forward.y = 0;
        forward.Normalize();

        //起点偏移,角色前方，避免检测到自己
        // forwardOffset = controller.radius; //额外偏移一个半径
        float forwardOffset = forwardOffsetBase+controller.radius; //额外偏移一个半径
        Vector3 baseOrigin = transform.position + forward * forwardOffset;

        float maxCheckHeight = controller.height * endHeightMult + minThroughHeight;   //最大检测高度，为墙体高度加上最小通过高度
        float lowClimbHeight = controller.height * lowClimbHeightMult;//低攀爬高度
        float minClimbHeight = controller.height * minClimbHeightMult; //最小攀爬高度

        float currentHeight = startHeight;
        bool hasHit = false;    //是否命中
        float highestHitRayY = 0;    //最高命中的高度
        RaycastHit highestHit = new RaycastHit();     //记录命中的信息 

        //存储每个高度的检测结果
        List<bool> hitFlags = new List<bool>();
        //空洞信息
        float holeStartHeight = -1;
        bool inHole = false;
        bool canClimb = false;//是否可以攀爬

        while(currentHeight <= maxCheckHeight+vertiicalStep)//防止错过最大值
        {
            Vector3 origin = baseOrigin + Vector3.up * currentHeight;
            if (Physics.Raycast(origin, forward, out RaycastHit hitInfo, forwardCheckDistance, climbableLayer))
            {
                Debug.DrawLine(origin, hitInfo.point, Color.red, 0.1f);//绘制射线
                inHole = false;
                hasHit = true;
                highestHitRayY = currentHeight;//记录最高命中高度上一个高度，为了向下检测准确墙顶高度
                highestHit = hitInfo;
                hitFlags.Add(true);
                wallNormal = hitInfo.normal;//记录墙面法线
                //如果当前处于空洞中，则记录空洞结束高度
            }
            else
            {
                //如果当前没有命中，则记录空洞开始高度
                if(!inHole)
                {
                    holeStartHeight = currentHeight;
                }
                else if(currentHeight - holeStartHeight >= minThroughHeight)
                {
                    //洞口高度足够，说明存在可以攀爬的最小空间，可以攀爬，最高命中高度为上一次的命中高度
                    if (hasHit)
                    {
                        canClimb = true;
                        break;
                    }
                }     
                hitFlags.Add(false);
                inHole = true;
            }
            currentHeight += vertiicalStep;
            
        }

        //若不能攀爬或者最高命中高度小于最小攀爬高度，则返回false
        if (!canClimb || highestHitRayY < minClimbHeight)
        {
            return false;
        }

        //精确检测墙顶高度，从最高命中高度向下检测，找到墙顶的准确高度
        float preciseWallTopHeight = highestHitRayY;    //记录墙顶高度
        bool canVault = true; //是否可以翻越

        //从空洞开始高度向下检测
        Vector3 wallTopCheckOrigin = Vector3.up * (holeStartHeight-highestHit.point.y)+  highestHit.point ; //从空洞开始高度向下检测
        Debug.DrawLine(wallTopCheckOrigin, wallTopCheckOrigin + Vector3.down * vertiicalStep*2, Color.yellow, 2f); //绘制墙顶点的调试线

        if (Physics.Raycast(wallTopCheckOrigin+forward * maxVaultWidth, Vector3.down, out RaycastHit wallTopHit, vertiicalStep*2, climbableLayer))
        {
            
            //如果检测到墙顶，则说明墙顶宽度大于最大翻越宽度，不能翻越
            canVault = false;
        }
        if (Physics.Raycast(wallTopCheckOrigin +forward*0.05f+ Vector3.up * vertiicalStep, Vector3.down, out RaycastHit wallTopHit2, vertiicalStep*2, climbableLayer))
        {
            Debug.DrawLine(wallTopCheckOrigin + forward*0.05f+ Vector3.up * vertiicalStep, wallTopHit2.point, Color.green, 2f); //绘制墙顶点的调试线

            preciseWallTopHeight = wallTopHit2.point.y;
            wallPoint = wallTopHit2.point;
        }
        else
        {
            Debug.LogWarning("无法精确检测墙顶高度，使用最高命中高度作为墙顶高度");
            wallPoint = highestHit.point;
        }
        // Debug.DrawLine(wallPoint, wallPoint + Vector3.up * 0.5f, Color.blue, 2f); //绘制墙顶点的调试线

        //计算攀爬类型
        if (preciseWallTopHeight - transform.position.y <= lowClimbHeight)
        {
            climbType = canVault ? ClimbType.Vault : ClimbType.LowClimb;

        }
        else
        {
            climbType = ClimbType.HighClimb;
        }
        
        return true;
    }


}
