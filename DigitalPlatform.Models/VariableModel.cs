using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.Models
{
    public class VariableModel : ObservableObject
    {

        public string DeviceNum { get; set; }
        // 唯一编码
        public string VarNum { get; set; }
        // 名称
        public string VarName { get; set; }
        // 地址
        public string VarAddress { get; set; }

        DataTable table = new DataTable();
        // 读取返回数据
        private object _value;
        public object Value
        {
            get { return _value; }
            set
            {
                Set(ref _value, value);
                //if (_value == value || AlarmConditions.Count == 0) return;
                ////在这里判断条件 可以很方便获取condition
                

                //foreach (var ac in AlarmConditions)
                //{
                //    string exp = value.ToString() + ac.Operator + ac.CompareValue;


                //    if (bool.TryParse(table.Compute(exp, "").ToString(), out bool result) && result)
                //    {
                //        //多个条件要找到最接近的警报
                //        //1、当前已经有了 命中条件
                //        //2、所有的同符号的拿出来 进行比对
                //        //3、小于的时候 拿出来所有满足条件的（小于的），找最小。大于就找最大
                //        var cm = this.CompareValues(ac.Operator, value);
                //        //两种方式 处理
                //        //1、把报警逻辑写到MainViewModel
                //        //2、消息
                //        //多处接受

                //        //在这里 把DeviceNum作为消息的Token
                //        Messenger.Default.Send<DeviceAlarmModel>(new DeviceAlarmModel
                //        {
                //            ANum = "A-" + DeviceNum + "-" + DateTime.Now.ToString("yyyyMMddHHmmssFFF"),
                //            CNum = cm.CNum,
                //            DNum = DeviceNum,
                //            VNum = VarNum,
                //            AlarmValue = value.ToString(),
                //            AlarmContent = cm.AlarmContent,
                //            DateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                //            Level = 1,
                //            State = 0
                //        }, "Alarm");
                //        break;
                //    }
                //}
            }

        }

        private ConditionModel CompareValues(string operatorStr, object currentValue)
        {
            //根据操作符找出  条件  并进行运算 判断运算结果能否转成bool型
            //如果转成了bool  teyparse返回true   在跟res判断  是否超出条件
            //这里找出了 所有满足条件的值
            var query = (from q in this.AlarmConditions
                         where q.Operator == operatorStr &&
                          Boolean.TryParse(
                          new DataTable().Compute(currentValue + q.Operator + q.CompareValue.ToString(), "").ToString(),
                          out bool state) &&
                          state
                         select q).ToList();
            //如果找到了
            if (query.Count > 0)
            {
                if (operatorStr == "<" || operatorStr == "<=")
                {
                    currentValue = AlarmConditions.Min(a => double.Parse(a.CompareValue));
                }
                else if (operatorStr == ">" || operatorStr == ">=")
                {
                    currentValue = AlarmConditions.Max(a => double.Parse(a.CompareValue));
                }
                return query.FirstOrDefault(v => v.CompareValue == currentValue.ToString());
            }

            return query[0];

        }
        // 偏移量
        public double Offset { get; set; } = 1;
        // 系数
        public double Modulus { get; set; } = 1;

        //数据类型
        public string VarType { get; set; } = "UInt16";
        public ObservableCollection<ConditionModel> AlarmConditions { get; set; }
        public ObservableCollection<ConditionModel> UnionConditions { get; set; }
        public RelayCommand AddConditionCommand { get; set; }

        public RelayCommand AddUnionConditionsCommand { get; set; }

        public RelayCommand<object> DeleteConditionCommand { get; set; }

        public VariableModel()
        {
            AlarmConditions = new ObservableCollection<ConditionModel>();
            UnionConditions = new ObservableCollection<ConditionModel>();
            AddConditionCommand = new RelayCommand(() =>
            {
                AlarmConditions
                .Add(new ConditionModel() { Operator = "<", CNum = "C" + DateTime.Now.ToString("yyyyMMddHHmmssFFF") });
            });
            AddUnionConditionsCommand = new RelayCommand(() =>
            {
                UnionConditions.Add(new ConditionModel()
                {
                    Operator = "<",
                    CNum = "C" + DateTime.Now.ToString("yyyyMMddHHmmssFFF")
                });
            });
            DeleteConditionCommand = new RelayCommand<object>(obj =>
            {
                UnionConditions.Remove(obj as ConditionModel);
            });

        }


    }
}
