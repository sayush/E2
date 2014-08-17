using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using LumenWorks.Framework.IO.Csv;
using System.IO;

namespace E2Data
{
    public class CsvImporter: IDataImporter
    {
        public DataTable Import(String path)
        {
            var dataTable = new DataTable();
            using(CachedCsvReader csv = new CachedCsvReader(new StreamReader(path), true)){
                int fieldCount = csv.FieldCount;

                string[] headers = csv.GetFieldHeaders();

                for (int i = 0; i < fieldCount; i++)
                {
                    dataTable.Columns.Add(headers[i]);
                    switch (headers[i])
                    {
                        case "Lat":
                            dataTable.Columns[i].DataType = System.Type.GetType("System.Double");
                            break;
                        case "Lng":
                            dataTable.Columns[i].DataType = System.Type.GetType("System.Double");
                            break;
                        case "Date":
                            dataTable.Columns[i].DataType = System.Type.GetType("System.DateTime");
                            break;
                        default:
                            dataTable.Columns[i].DataType = System.Type.GetType("System.String");
                            break;
                    }

                }

                while (csv.ReadNextRecord())
                {
                    DataRow dr = dataTable.NewRow();
                    for (int i = 0; i < fieldCount; i++)
                        dr[headers[i]] = csv[i].ToString().Replace("\"", "");

                    bool missing = false;
                    for (int i = 0; i < fieldCount; i++)
                    {
                        missing = dr[i].ToString() == "" ? true : false;
                    }
                    if(!missing) dataTable.Rows.Add(dr);
                }
            }
            return dataTable;
        }
    }
}
