using System;
using System.Data.Common;
using EntityFramework.Audit;

namespace Utile.Money.Extended
{
    //todo try to create a simpler way to overide way this formatter is specified by the property attribute
    public static class MoneyFormat
    {
        public static object FormatMoney(AuditPropertyContext auditProperty)
        {
            var d = auditProperty.Value as DbDataRecord;
            if (d == null)
                return null;
            try
            {
                var m = (Money)d;
                return m;
            }
            catch (InvalidCastException)
            {
                return null;
            }
        }
    }
}
