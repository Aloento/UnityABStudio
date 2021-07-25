namespace SoarCraft.QYun.UnityABStudio.UnitTest {
    using System;
    using System.IO;
    using System.Reflection;

    public static class Helpers {
        public static bool ByteArrayToFile(string fileName, byte[] byteArray) {
            using var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            fs.Write(byteArray, 0, byteArray.Length);
            return true;
        }

        public class PrivateObject {
            private readonly Type o;

            public PrivateObject(Type o) => this.o = o;

            public object Invoke(string methodName, params object[] args) {
                var methodInfo = o.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                if (methodInfo == null) {
                    throw new Exception($"Method'{methodName}' not found is class '{o.GetType()}'");
                }
                return methodInfo.Invoke(o, args);
            }
        }
    }
}
