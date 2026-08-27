using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class UIBasePanel : MonoBehaviour
{
   private Dictionary<string,UIBehaviour> controlDict = new Dictionary<string, UIBehaviour>();
   private List<string> defaultSelectedList = new List<string>()
   {
      "Image",
      "Text (TMP)",
      "RawImage",
      "Background",
      "Checkmark",
      "Label",
      "Text (Legacy)",
      "Arrow",
      "Placeholder",
      "Fill",
      "Handle",
      "Viewport",
      "Scrollbar Horizontal",
      "Scrollbar Vertical"
   };
   protected virtual void Awake()
   {
      //为了避免 某一个对象上存在两种控件的情况
      //我们应该优先查找重要的组件
      FindChildControl<Button>();
      FindChildControl<Toggle>();
      FindChildControl<Slider>();
      FindChildControl<InputField>();
      FindChildControl<ScrollRect>();
      FindChildControl<Dropdown>();
      //即使对象上挂在了多个组件 只要优先找到了重要组件
      //之后也可以通过重要组件得到身上其他挂载的内容
      FindChildControl<Text>();
      FindChildControl<TextMeshProUGUI>();
      FindChildControl<Image>();
   }

   /// <summary>
   /// 显示面板时调用的函数
   /// </summary>
   public abstract void ShowMe();

   /// <summary>
   /// 隐藏面板时调用的函数
   /// </summary>
   public abstract void HideMe();
   /// <summary>
   /// 寻找子控件
   /// </summary>
   /// <typeparam name="T"></typeparam>
   private void FindChildControl<T>() where T : UIBehaviour
   {
      T[] controls = GetComponentsInChildren<T>(true);
      foreach (var control in controls)
      {
         if (!controlDict.ContainsKey(control.name))
         {
            if(!defaultSelectedList.Contains(control.name))
            {
               controlDict.Add(control.name, control);
               //添加事件监听
               switch (control)
               {
                  case Button btn:
                     btn.onClick.AddListener(() => ClickBtn(btn.name));
                     break;
                  case Slider slider:
                     slider.onValueChanged.AddListener((value) => SliderValueChange(slider.name, value));
                     break;
                  case Toggle toggle:
                     toggle.onValueChanged.AddListener((value) => ToggleValueChange(toggle.name, value));
                     break;
                  case InputField inputField:
                     inputField.onValueChanged.AddListener((value) => InputFieldValueChange(inputField.name, value));
                     break;
                  case TMP_InputField tmpInputField:
                     tmpInputField.onValueChanged.AddListener((value) => InputFieldValueChange(tmpInputField.name, value));
                     break;
                  case Dropdown dropdown:
                     dropdown.onValueChanged.AddListener((value) => DropdownValueChange(dropdown.name, value));
                     break;
                  //TODO: 其他组件的事件监听
               default:
                     break;
                  
               }
            }
         }
      }
   }
   /// <summary>
   /// 获取指定名字以及指定类型的组件
   /// </summary>
   /// <typeparam name="T">组件类型</typeparam>
   /// <param name="name">组件名字</param>
   /// <returns></returns>
   public T GetControl<T>(string name) where T : UIBehaviour
   {
      if (controlDict.ContainsKey(name))
      {
         return controlDict[name] as T;
      }
      else
      {
         Debug.LogError($"控件{typeof(T)}:{name}不存在");
         return null;
      }
   }

   protected virtual void ClickBtn(string btnName)
   {

   }
   protected virtual void SliderValueChange(string sliderName, float value)
   {

   }
   protected virtual void ToggleValueChange(string toggleName, bool value)
   {

   }
   protected virtual void InputFieldValueChange(string inputFieldName, string value)
   {

   }
   protected virtual void DropdownValueChange(string dropdownName, int value)
   {

   }
   


}
