using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.Assets
{
    public class ActionManager
    {
        private ActionManager() { }
        static Dictionary<string, Delegate> actionMap = new Dictionary<string, Delegate>();

        public static void Register<T>(string key, Delegate d)
        {
            if (!actionMap.ContainsKey(key))
                actionMap.Add(key, d);
        }

        public static void Unregister(string key)
        {
            if (actionMap.ContainsKey(key))
                actionMap.Remove(key);
        }

        // 单执行
        public static void Execute<T>(string key, T data)
        {
            if (actionMap.ContainsKey(key))
                actionMap[key].DynamicInvoke(data);
        }

        public static void Execute(string key)
        {
            if (actionMap.ContainsKey(key))
                actionMap[key].DynamicInvoke();
        }
        // 执行并返回状态
        public static bool ExecuteAndResult<T>(string key, T data)
        {
            if (actionMap.ContainsKey(key))
            {
                var action = (actionMap[key] as Func<T, bool>);
                if (action == null)
                    return false;

                return action.Invoke(data);
            }
            return false;
        }
    }
}
