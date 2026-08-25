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

        float minStandHeight = controller.height+0.1f; //最小站立高度，低于这个高度无法攀爬
        float maxCheckHeight = controller.height * endHeightMult + minStandHeight;   //最大检测高度，为墙体高度加上最小站立高度
        float lowClimbHeight = controller.height * lowClimbHeightMult;//低攀爬高度
        float minClimbHeight = controller.height * minClimbHeightMult; //最小攀爬高度

        float currentHeight = startHeight;
        bool hasHit = false;    //是否命中
        float highestHitRayY = 0;    //最高命中的高度
        RaycastHit highestHit = new RaycastHit();     //记录命中的信息 

        //空洞信息
        float holeStartHeight = -1;
        bool inHole = false;
        bool canVault = false; //是否可以翻越

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
                        Vector3 wallTopCheckOrigin = Vector3.up * vertiicalStep +  highestHit.point ; //从空洞开始高度向下检测
                        canVault = CheckVault(wallTopCheckOrigin,-wallNormal * maxVaultWidth, vertiicalStep*2, out RaycastHit vaultHit);
                        if(canVault && vaultHit.point.y <= lowClimbHeight+transform.position.y)
                        {
                            //可以翻越
                            climbType = ClimbType.Vault;
                            wallPoint = vaultHit.point;
                            return true;
                        }
                        if(currentHeight - holeStartHeight >= minStandHeight)
                        {
                            //洞口高度足够，说明存在可以攀爬的最小空间，可以攀爬，最高命中高度为上一次的命中高度
                            if(vaultHit.point.y > lowClimbHeight+transform.position.y)
                            {
                                //如果墙顶高度大于低攀爬高度，则为高攀爬
                                climbType = ClimbType.HighClimb;
                            }
                            else
                            {
                                //否则为低攀爬
                                climbType = ClimbType.LowClimb;
                            }
                            wallPoint = vaultHit.point;
                            return true;
                        }
                    }
                }     
                inHole = true;
            }
            currentHeight += vertiicalStep;   
        }
        return false;
    }
    /// <summary>
    /// 检查是否可以翻越
    /// </summary>
    /// <param name="CheckOrigin">起点</param>
    /// <param name="forward">前进向量</param>
    /// <param name="distance">向下检测距离</param>
    /// <param name="hitInfo">返回检测信息</param>
    /// <returns></returns>
    bool CheckVault(Vector3 CheckOrigin, Vector3 forward, float distance,out RaycastHit hitInfo)
    {
        Debug.DrawLine(CheckOrigin, CheckOrigin+Vector3.down*distance, Color.blue, 0.1f); //绘制前检测线
        Debug.DrawLine(CheckOrigin+forward, CheckOrigin+forward+Vector3.down*distance, Color.black, 0.1f);//绘制后检测线
    //检查墙顶是否可以翻越
      Physics.Raycast(CheckOrigin, Vector3.down, out hitInfo, distance, climbableLayer);
      return !Physics.Raycast(CheckOrigin+forward,Vector3.down, distance, climbableLayer);  //检测墙顶前方是否为墙,如果有墙则不能翻越
    }


}
