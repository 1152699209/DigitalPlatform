using DigitalPlatform.IDAL;
using DigitalPlatform.Models;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.ViewModels
{
    public class AlarmViewModel : ViewModelBase
    {
        public ObservableCollection<AlarmModel> Alarms { get; set; } = new ObservableCollection<AlarmModel>();

        private string _keyValue;
        public RelayCommand ExportCommand { get; set; }
        public string KeyValue
        {
            get { return _keyValue; }
            set { Set(ref _keyValue, value); }
        }

        public RelayCommand RefreshCommand { get; set; }

        ILocalDataAccess _localDataAccess;
        public AlarmViewModel(ILocalDataAccess localDataAccess)
        {
            _localDataAccess = localDataAccess;
            RefreshCommand = new RelayCommand(RecordInit);
            RecordInit();
            ExportCommand = new RelayCommand(Export);
        }
        private void Export()
        {
            //SaveFileDialog
            // 1、参照停车场项目中的导出逻辑  :  NPOI

            // 2、导出CSV
            //List<DataGridColumnModel> cs = Alarms.Where(c => c.IsSelected).OrderBy(c => c.Index).ToList();

            //string datas = string.Join(",", cs.Select(c => c.Header)) + "\r\n";
            //foreach (var item in Alarms)
            //{
            //    foreach (var col in cs)
            //    {
            //        PropertyInfo pi = item.GetType().GetProperty(col.BindingPath, BindingFlags.Instance | BindingFlags.Public);

            //        var v = pi.GetValue(item);
            //        datas += v == null ? "," : v.ToString() + ",";
            //        datas.Remove(datas.Length - 1);
            //    }
            //    datas += "\r\n";
            //}

            //System.IO.File.WriteAllText("data.csv", datas, Encoding.UTF8);
        }
        private void RecordInit()
        {
            Alarms.Clear();
            var rs = _localDataAccess.GetAlarmList(this.KeyValue);
            foreach (var record in rs)
            {
                Alarms.Add(new AlarmModel(_localDataAccess)
                {
                    Index = Alarms.Count + 1,
                    id = record.id,
                    DeviceNum = record.DeviceNum,
                    DeviceName = record.DeviceName,
                    VariableNum = record.VariableNum,
                    VariableName = record.VariableName,
                    RecordValue = record.RecordValue,
                    State = record.State,
                    AlarmNum = record.AlarmNum,
                    DataTime = record.RecordTime,
                    SolveTime = record.SolveTime,
                    AlarmContent = record.AlarmContent,
                    AlarmLevel = record.AlarmLevel,
                    UserId = record.UserId,
                    UserName = record.UserName
                });
            }
        }
    }
}
