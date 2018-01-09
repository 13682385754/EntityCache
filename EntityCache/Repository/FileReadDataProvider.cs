using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EntityCache.Repository
{
    internal class FileReadDataProvider : IRepositoryProvider
    {
        private const string FileName = "PersonRepository.txt";

        public bool Add(Dictionary<string, string> entity)
        {
            WritePropertiesToLine(entity);
            return true;
        }

        public bool Update(Dictionary<string, string> entity)
        {
            WritePropertiesToLine(entity);
            return true;
        }

        public bool Remove(int id)
        {
            if (CountLines() > id)
            {
                ChangeLine(string.Empty, id);
                return true;
            }

            return false;
        }
       
        public Dictionary<string, string> Get(int id)
        {
            return ReadLine(id);
        }

        List<Dictionary<string, string>> IRepositoryProvider.GetAllEntries()
        {
            string[] lines = File.ReadAllLines(FileName);
            return lines.Select((t, i) => ParseLine(i, t)).Where(y => y != null).ToList();
        }

        public void ClearData()
        {
            File.WriteAllText(FileName, string.Empty);
        }

        public void InsertMockDataIntoRepo()
        {
            string data =
@"Name,aaa,Age,10
Name,bbb,Age,21
Name,ccc,Age,22
Name,ddd,Age,13
Name,eee,Age,14
Name,fff,Age,15
Name,ggg,Age,16
Name,hhh,Age,17
Name,iii,Age,18
Name,jjj,Age,19";
            File.WriteAllText(FileName, data);
        }

        public string GetFileContents()
        {
            return File.ReadAllText(FileName);
        }

        private static void WritePropertiesToLine(Dictionary<string, string> properties)
        {
            int lineNumber = int.Parse(properties["Id"]);
            properties.Remove("Id");
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var property in properties)
            {
                stringBuilder.Append(property.Key);
                stringBuilder.Append(',');
                stringBuilder.Append(property.Value);
                stringBuilder.Append(',');
            }

            stringBuilder.Remove(stringBuilder.Length - 1, 1);
            ChangeLine(stringBuilder.ToString(), lineNumber);
        }

        private static Dictionary<string, string> ReadLine(int lineNumber)
        {
            string[] lines = ReadAllLines();
            
            return lines.Length > lineNumber ? ParseLine(lineNumber, lines[lineNumber]) : null;
        }

        private static Dictionary<string, string> ParseLine(int lineNumber, string line)
        {
            // lines are in format of comma separated key, value
            // line number is the id

            line = line.Replace(" ", string.Empty);
            string[] parts = line.Split(',');
            if (parts.Length == 1)
            {
                return null;
            }

            var props = new Dictionary<string, string> {{"Id", lineNumber.ToString()}};
            for (var i = 0; i < parts.Length; i += 2)
            {
                props.Add(parts[i], parts[i+1]);
            }

            return props;
        }

        private static void ChangeLine(string newText, int lineNumber)
        {
            List<string> arrLine = new List<string>(ReadAllLines());

            if (arrLine.Count <= lineNumber)
            {
                arrLine.Add(newText);
            }
            else
            {
                arrLine[lineNumber] = newText;
            }

            File.WriteAllLines(FileName, arrLine);
        }

        private static string[] ReadAllLines()
        {
            return File.ReadAllLines(FileName);
        }

        private static int CountLines()
        {
            return ReadAllLines().Length;
        }
    }
}
