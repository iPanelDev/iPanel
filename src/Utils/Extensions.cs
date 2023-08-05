using System.Threading.Tasks;

namespace iPanelHost.Utils
{
    internal static class Extensions
    {
        /// <summary>
        /// 等待
        /// </summary>
        public static void Await(this Task task) => task.GetAwaiter().GetResult();

        /// <summary>
        /// 等待
        /// </summary>
        public static T Await<T>(this Task<T> task) => task.GetAwaiter().GetResult();
    }
}