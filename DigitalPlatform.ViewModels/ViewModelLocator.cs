using CommonServiceLocator;
using DigitalPlatform.DataAccess;
using DigitalPlatform.IDAL;
using DigitalPlatform.ViewModels;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Zhaoxi.DigitaPlatform.Entities;

namespace DigitalPlatform.ViewModels
{
    public class ViewModelLocator
    {
        public static SimpleIoc MyIoc { get;} = SimpleIoc.Default;
        public ViewModelLocator()
        {
            
            //向Ioc容器
            MyIoc.Register<LoginViewModel>();
            MyIoc.Register<MainViewModel>();
            MyIoc.Register<ConfigViewModel>();
            MyIoc.Register<ConditionDialogViewModel>();
            MyIoc.Register<TrendViewModel>();
            MyIoc.Register<ILocalDataAccess, LocalDataAccess>();
            MyIoc.Register<AlarmViewModel>(); 
            MyIoc.Register<ReportViewModel>();
            MyIoc.Register<SettingsViewMdole>();
        }
        public LoginViewModel LoginViewModel => MyIoc.GetInstance<LoginViewModel>();
        public MainViewModel MainViewModel => MyIoc.GetInstance<MainViewModel>();

        public ConfigViewModel ConfigViewModel => MyIoc.GetInstance<ConfigViewModel>();

        public AlarmViewModel AlarmViewModel => MyIoc.GetInstance<AlarmViewModel>();

        public ConditionDialogViewModel ConditionDialogViewModel=> MyIoc.GetInstance<ConditionDialogViewModel>();
        public TrendViewModel TrendViewModel => MyIoc.GetInstance<TrendViewModel>();
        public ReportViewModel ReportViewModel => MyIoc.GetInstance<ReportViewModel>();
        public SettingsViewMdole SettingsViewModel => MyIoc.GetInstance<SettingsViewMdole>();
        

        // 通过ViewModelLocator对象实例 进行对应的VM对象的销毁
        public static void Cleanup<T>() where T : ViewModelBase
        {
            //如果当前类型 已经注册
            if (SimpleIoc.Default.IsRegistered<T>() && SimpleIoc.Default.ContainsCreated<T>())
            {
                var instances = SimpleIoc.Default.GetAllCreatedInstances<T>();
                foreach (var instance in instances)
                {

                    instance.Cleanup();
                }
                //移除对象
                SimpleIoc.Default.Unregister<T>();
                //再注册一个新的
                SimpleIoc.Default.Register<T>();
            }
        }

        private static List<RecordWriteEntity> records = new List<RecordWriteEntity>();
        public static void AddRecord(RecordWriteEntity record)
        {
            if (record != null && record != null)
                records.Add(record);
            //添加null时  说明程序要退出了
            if (record == null || records.Count >= 200)
            {
                MyIoc.GetInstance<ILocalDataAccess>().SaveRecord(records);
                records.Clear();
            }
        }



    }
}
