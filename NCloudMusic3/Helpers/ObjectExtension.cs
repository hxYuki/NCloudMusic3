using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCloudMusic3.Helpers {
    public static class ObjectExtension {
        public static void PublicShallowCopy<T>(this T self, T target, params string[] exceptions) {
            foreach (var field in typeof(T).GetFields()) {
                if (exceptions?.Contains(field.Name) ?? false)
                    continue;
                field.SetValue(self, field.GetValue(target));
            }
            foreach (var p in typeof(T).GetProperties()) {
                if (exceptions?.Contains(p.Name) ?? false)
                    continue;
                p.SetValue(self, p.GetValue(target));
            }
        }
    }
}
