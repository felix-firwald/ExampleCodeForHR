using Incas.Core.Classes;
using Incas.Templates.Components;
using IncasEngine;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Incas.Templates.Models
{
    public struct TagParameters
    {
        public string Name;
    }

    public class Tag : Model
    {
        public int id { get; set; }
        public int template { get; set; }
        public string name { get; set; }
        public string visibleName { get; set; }
        public TagType type { get; set; }
        public string value { get; set; }

        public int orderNumber { get; set; }
        public string description { get; set; }
        public string command { get; set; }
        public Tag()
        {
            this.tableName = "Tags";
        }

        public List<Tag> GetAllTagsByTemplate(int templ, string parents = "0")
        {
            List<string> parentsList = parents.Split(';').ToList();
            parentsList.Add(templ.ToString());
            DataTable dt = this.StartCommand()
                .Select()
                .WhereIn(nameof(this.template), parentsList)
                .OrderByASC("template ASC, orderNumber")
                .Execute();

            List<Tag> result = [];
            foreach (DataRow dr in dt.Rows)
            {
                Tag mt = new();
                mt.Serialize(dr);
                mt.type = (TagType)Enum.Parse(typeof(TagType), dr["type"].ToString());
                result.Add(mt);
            }
            return result;
        }

        public void AddTag()
        {
            this.StartCommand()
                .Insert(new Dictionary<string, string>
                {
                    { "template", this.template.ToString() },
                    { "name", this.name },
                    { "visibleName", this.visibleName },
                    { "type", this.type.ToString() },
                    { "value", this.value },
                    { "description", this.description },
                    { "command", this.command },
                    { "orderNumber", this.orderNumber.ToString() }
                })
                .ExecuteVoid();
        }
        public void UpdateTag()
        {
            if (this.id is 0)
            {
                this.AddTag();
                return;
            }
            this.StartCommand()
                .Update("name", this.name)
                .Update("visibleName", this.visibleName)
                .Update("type", this.type.ToString())
                .Update("value", this.value)
                .Update("description", this.description)
                .Update("command", this.command)
                .Update("orderNumber", this.orderNumber.ToString())
                .WhereEqual("id", this.id.ToString())
                .ExecuteVoid();
        }
        public void RemoveTag()
        {
            this.StartCommand()
                .Delete()
                .WhereEqual("id", this.id.ToString())
                .ExecuteVoid();
        }
        public void RemoveAllTagsByTemplate(int templ)
        {
            this.StartCommand()
                .Delete()
                .WhereEqual("template", templ.ToString())
                .ExecuteVoid();
        }
        // для работы функции ниже должен быть предварительно обновлен template
        public void GetChild()
        {
            DataRow dr = this.StartCommand()
                            .Select()
                            .WhereEqual("template", this.template.ToString())
                            .WhereEqual("parent", this.id.ToString())
                            .ExecuteOne();
            if (dr != null)
            {
                this.Serialize(dr);
                this.type = (TagType)Enum.Parse(typeof(TagType), dr["type"].ToString());
            }
        }
        public string GetKey()
        {
            return "TAG";
        }
        public CommandSettings GetCommand()
        {
            try
            {
                return JsonConvert.DeserializeObject<CommandSettings>(Cryptographer.DecryptString(this.command));
            }
            catch (Exception)
            {
                return new();
            }
        }
        public void SaveCommand(CommandSettings cs)
        {
            this.command = Cryptographer.EncryptString(JsonConvert.SerializeObject(cs));
        }
    }
}