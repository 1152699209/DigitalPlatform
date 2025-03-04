using DigitalPlatform.IDAL;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.Models
{
    public class AlarmModel : ObservableObject
    {
        public int Index { get; set; }
        public string id { get; set; }
        public string DeviceNum { get; set; }
        public string DeviceName { get; set; }
        public string VariableNum { get; set; }
        public string VariableName { get; set; }

        public RelayCommand CancelAlarmCommand { get; set; }
        public string RecordValue { get; set; }
        private string _state;

        public string State
        {
            get { return _state; }
            set
            {
                Set(ref _state, value);
                if (value == "0")
                    StateName = "未处理";
                else if (value == "1")
                    StateName = "已处理";
                this.RaisePropertyChanged("StateName");
            }
        }
        public string StateName { get; set; }

        public string AlarmNum { get; set; }
        public string DataTime { get; set; }
        private string _solveTime;

        public string SolveTime
        {
            get { return _solveTime; }
            set { Set(ref _solveTime, value); }
        }

        public string AlarmContent { get; set; }
        public string AlarmLevel { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        private ILocalDataAccess _localDataAccess;
        public AlarmModel(ILocalDataAccess localDataAccess)
        {
            _localDataAccess = localDataAccess;
            //第一个参数为 逻辑  第二个参数是对应能否执行
            CancelAlarmCommand = new RelayCommand(DoCancelAlarm,DoCanCancelAlarm);
        }

        private void DoCancelAlarm()
        {
            this.State = "1";
            //通过事件  检查是否能继续执行命令
            //在这里给SolveTime赋值 触发通知 直接在View显示
            this.SolveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            
            CancelAlarmCommand.RaiseCanExecuteChanged();
            _localDataAccess.UpdateAlarmState(this.id,this.SolveTime);
            //取消报警后 还要通知对应界面的设备
            Messenger.Default.Send(this.DeviceNum, "CancelAlarm");
            
        }
        private bool DoCanCancelAlarm()
        {
            //未处理的才能进行处理
            return this.State == "0";
        }
    }

}
