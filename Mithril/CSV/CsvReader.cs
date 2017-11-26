using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Mithril
{
    public static class CsvReader
    {
        public static T[] Read<T>(String filePath) where T : class, ICsvEntry, new()
        {
            using (Stream input = File.OpenRead(filePath))
                return Read<T>(input);
        }

        private static T[] Read<T>(Stream input) where T : class, ICsvEntry, new()
        {
            LinkedList<Exception> exceptions = new LinkedList<Exception>();
            LinkedList<T> entries = new LinkedList<T>();
            using (StreamReader sr = new StreamReader(input))
            {
                StringBuilder sb = new StringBuilder();

                while (!sr.EndOfStream)
                {
                    String line = sr.ReadLine();
                    if (String.IsNullOrEmpty(line) || line[0] == '#')
                        continue;

                    try
                    {
                        List<String> cells = new List<String>();

                        Boolean escaped = false;
                        Boolean escapes = false;
                        for (var index = 0; index < line.Length; index++)
                        {
                            Char ch = line[index];
                            if (ch == '"')
                            {
                                if (sb.Length == 0)
                                {
                                    escapes = true;
                                    continue;
                                }

                                if (!escapes)
                                {
                                    sb.Append('"');
                                    continue;
                                }

                                if (escaped)
                                {
                                    sb.Append('"');
                                    escaped = false;
                                    continue;
                                }
                                else
                                {
                                    escaped = true;
                                }
                            }
                            else if (ch == ';')
                            {
                                if (!escapes || escaped)
                                {
                                    cells.Add(sb.ToString());
                                    sb.Clear();
                                    escaped = false;
                                }
                                else
                                {
                                    sb.Append(';');
                                }
                            }
                            else if (escaped)
                            {
                                throw new InvalidDataException(line);
                            }
                            else
                            {
                                sb.Append(ch);
                            }
                        }

                        if (sb.Length > 0)
                        {
                            cells.Add(sb.ToString());
                            sb.Clear();
                        }

                        String[] raw = cells.ToArray();
                        for (Int32 i = 0; i < raw.Length; i++)
                        {
                            String col = raw[i];
                            if (col.Length > 0 && col[0] == '#')
                            {
                                Array.Resize(ref raw, i);
                                break;
                            }
                        }

                        T entry = new T();
                        entry.ParseEntry(raw);
                        entries.AddLast(entry);
                    }
                    catch (Exception ex)
                    {
                        exceptions.AddLast(new CsvParseException($"Failed to parse [{typeof(T).Name}] from line [{line}].", ex));
                        entries.AddLast((T)null);
                    }
                }
            }

            if (exceptions.Count == 0)
                return entries.ToArray();

            if (exceptions.Count == 1)
                throw exceptions.First.Value;

            throw new AggregateException(exceptions);
        }
    }
}