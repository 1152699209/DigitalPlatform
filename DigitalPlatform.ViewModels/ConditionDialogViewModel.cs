using DigitalPlatform.Assets;
using DigitalPlatform.Models;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.ViewModels
{
    public class ConditionDialogViewModel : ViewModelBase
    {
        public List<ConditionOperatorModel> Operators { get; set; } = new List<ConditionOperatorModel>();
        public List<DeviceDropModel> DeviceDropList { get; set; }

        public ConditionDialogViewModel()
        {
            if (!IsInDesignMode)
            {
                // 只处理基本的逻辑运算     扩展：组件逻辑处理   &&   ||    ()
                Operators.Add(new ConditionOperatorModel() { Header = "大于", Value = ">" });
                Operators.Add(new ConditionOperatorModel() { Header = "小于", Value = "<" });
                Operators.Add(new ConditionOperatorModel() { Header = "等于", Value = "==" });
                Operators.Add(new ConditionOperatorModel() { Header = "大于等于", Value = ">=" });
                Operators.Add(new ConditionOperatorModel() { Header = "小于等于", Value = "<=" });
                Operators.Add(new ConditionOperatorModel() { Header = "不等于", Value = "!=" });
                //1 主动发送请求 对方接收后再相应
                //1.1
                //2 把其他ViewModel中的数据 注入到其中
                //把Doget方法作为参数 传给注册名为GetDevice的方法
                ActionManager.Execute(
                    "GetDevice",
                    DoGet
                );
            }
        }

        private void DoGet(Func<List<DeviceDropModel>> func)
        {
            DeviceDropList = func.Invoke();
        }
    }
}
