using Common;
using Incas.Core.Classes;
using System;
using System.Collections.Generic;
using System.Data;
//using Microsoft.Office.Core;

namespace Incas.Core.Classes
{
    public enum DatabasePermissions
    {
        All,
        AdminOnly,
        ModeratorAndAdmin,
        ModeratorOnly,
        ModeratorAndOperator,
        OperatorOnly
    }
    public enum Types
    {
        INT,
        STR
    }

    public class InvalidIstructionException : Exception
    {
        public InvalidIstructionException(string message) : base(message)
        {

        }
    }
    public interface IModelConverter
    {
        public Model AsModel();
    }

    public abstract class Model : IDisposable
    {
        private bool disposed = false;
        protected string tableName { get; set; }

        #region static 
        public static Dictionary<Type, string> SqliteTypes = new Dictionary<Type, string>
        {
            { typeof(string), "STRING" },
            { typeof(int), "INT" },
            { typeof(DateTime), "DATETIME" },
            { typeof(bool), "BOOLEAN" },
        };
        #endregion
        public Model()
        {

        }
        public override string ToString()
        {
            return this.tableName;
        }
        #region Service
        public void Dispose()
        {
            this.CleanUp(true);
            GC.SuppressFinalize(this);
        }
        private void CleanUp(bool clean)
        {
            if (!this.disposed)
            {
                if (clean)
                {
                    this.disposed = true;
                }
            }
        }
        #endregion

        protected List<string> GetFields()
        {
            List<string> result = [];
            foreach (System.Reflection.PropertyInfo prop in this.GetType().GetProperties())
            {
                result.Add(prop.Name);
            }
            return result;
        }
        protected Query StartCommand()
        {
            return new Query(this.tableName);
        }
        protected Query StartCommandToService()
        {
            return new Query(this.tableName, DBConnectionType.SERVICE);
        }
        protected Query StartCommandToOtherPort(string port)
        {
            return new Query(this.tableName, DBConnectionType.OTHER)
            {
                DBPath = $"{ProgramState.ServerProcesses}\\{port}.incport"
            };
        }
        protected Query StartCommandToMyPort()
        {
            return new Query(this.tableName, DBConnectionType.OTHER)
            {
                DBPath = ServerProcessor.Port
            };
        }
        protected Query StartCommandToChat(string path)
        {
            return new Query(this.tableName, path);
        }
        protected Query StartCommandToCustom()
        {
            return new Query(this.tableName, DBConnectionType.CUSTOM);
        }

        #region Standart Requests
        protected DataTable GetAll()
        {
            return this.StartCommand().Select().Execute();
        }
        protected DataRow GetById(int id)
        {
            return this.StartCommand().Select().WhereEqual("id", id.ToString()).Execute().Rows[0];
        }
        protected DataTable GetByField(string field, string value)
        {
            return this.StartCommand().Select().WhereEqual(field, value).Execute();
        }
        protected void DeleteById(int id)
        {
            this.StartCommand().Delete().WhereEqual("id", id.ToString()).ExecuteVoid();
        }
        #endregion

        #region Connection, Request and Permission
        /// <summary>
        /// Возвращает SqlConnection с подставленными значениями, предварительно проверяя доступ
        /// </summary>
        /// <returns></returns>

        private bool CheckPermission(DatabasePermissions permission)
        {
            switch (permission)
            {
                case DatabasePermissions.All: return true;
                case DatabasePermissions.AdminOnly:
                    return Permission.IsUserHavePermission(PermissionGroup.Admin, PermissionMode.Monopoly);
                case DatabasePermissions.ModeratorAndAdmin:
                    return Permission.IsUserHavePermission(PermissionGroup.Moderator, PermissionMode.Rising);
                case DatabasePermissions.ModeratorOnly:
                    return Permission.IsUserHavePermission(PermissionGroup.Moderator, PermissionMode.Monopoly);
                case DatabasePermissions.ModeratorAndOperator:
                    return Permission.IsUserHavePermission(PermissionGroup.Moderator, PermissionMode.Cascade);
                case DatabasePermissions.OperatorOnly:
                    return Permission.IsUserHavePermission(PermissionGroup.Operator, PermissionMode.Monopoly);
                default: throw new Exception($"Unknown permission type \"{permission}\" got.");
            }
        }
        protected int BoolToInt(bool value)
        {
            return value ? 1 : 0;
        }
        protected bool IntToBool(int value)
        {
            return value != 0;
        }
        protected bool IntToBool(long value)
        {
            return value != 0;
        }
        protected bool IntToBool(uint value)
        {
            return value != 0;
        }
        /// <summary>
        /// Does not support custom types and sets such fields to null
        /// </summary>
        protected void Serialize(DataRow dr)
        {
            foreach (System.Reflection.PropertyInfo property in this.GetType().GetProperties())
            {
                try
                {
                    property.SetValue(this, Convert.ChangeType(dr[property.Name], property.PropertyType)); // берет из datarow по названию переменной столбец и устанавливает значение к свойству
                }
                catch (Exception)
                {
                    continue;
                }
            }
        }
        protected static T ParseEnum<T>(object value, T defaultValue)
        {
            string input = value.ToString();
            return string.IsNullOrEmpty(input) ? defaultValue : (T)Enum.Parse(typeof(T), value.ToString(), true);
        }

        #endregion

        private DataRow GetOne(DataTable dt)
        {
            try
            {
                return dt.Rows[0];
            }
            catch (Exception)
            {
                return null;
            }
        }
        protected int ParseCount(DataTable dt)
        {
            if (dt.Rows.Count == 0) return 0;
            DataRow dr = this.GetOne(dt);
            return int.Parse(dr["count"].ToString());
        }
    }
}

