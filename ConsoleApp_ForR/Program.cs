// NuGet Packages Necessary
// System.Text.Encoding.CodePages
// ExcelDataReader
// ExcelDataReader.DataSet

using System.Data;
using System.Diagnostics;
using System.Text;
using ExcelDataReader;

public class RandomDataToR
{
    public static void Main(string[] args)
    {
        // Register encoding provider
        Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);


        // Create data
        int row_cnt = 10;
        double x_bar = 50;
        double sd = 10;

        // Base directory of the application (bin\debug)
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;

        // Navigate up to the project root directory (assuming the executable is in bin/x64/Debug or bin/x64/Release)
        string projDir = Directory.GetParent(baseDir).Parent.Parent.Parent.Parent.FullName;

        // Provide file locations for Rscript.exe (note x64 folder) and rscript file
        string rAppPath = @"C:\Program Files\R\R-4.3.0\bin\x64\Rscript.exe";
        //string rScriptPath = Path.Combine(baseDirectory, "Utility", "get_output5.r");
        string rScriptPath = Path.Combine(projDir, "Utility", "eColi_sex.r");
        string excelFilePath = Path.Combine(projDir, "Utility", "ecoli.xlsx");

        // Generate random numbers with mean x and StdDev sd
        double[] data = GenerateRandomData(row_cnt, x_bar, sd);

        // Create a DataTable with random data for initial testing
        DataTable dt = CreateDataTable(data);

        // Process the random data
        //ProcessDataWithR(dt, rAppPath, rScript);

        // Process eColi data
        DataTable dtExcelData = ReadExcelFileToDataTable(excelFilePath);
        ProcessDataWithR(dtExcelData, rAppPath, rScriptPath);
    }

    private static double[] GenerateRandomData(int count, double mean, double standardDeviation)
    {
        double[] data = new double[count];
        var random = new Random();
        for (int i = 0; i < count; i++)
        {

            double u1 = 1.0 - random.NextDouble(); // uniform(0,1] random doubles
            double u2 = 1.0 - random.NextDouble();
            double rndStdNorm = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            data[i] = mean + standardDeviation * rndStdNorm;
        }

        return data;
    }

    private static DataTable CreateDataTable(double[] data)
    {
        DataTable dataTable = new DataTable();
        dataTable.Columns.Add("Data", typeof(double));
        foreach (double value in data)
        {
            dataTable.Rows.Add(Math.Round(value, 2));
        }
        return dataTable;
    }

    private static void ProcessDataWithR(DataTable dt, string exePath, string scriptPath)
    {
        // Call sendDataToR and store the result
        string results = sendDataToR(dt, exePath, scriptPath);

        // Check if results are not null and print them
        if (results != null)
        {
            Console.WriteLine(results);
        }
        else
        {
            Console.WriteLine("No results were returned from R script.");
        }
    }
    private static string sendDataToR(DataTable dataTable, string exePath, string rScriptPath)
    {

        try
        {
            // Serialize DataTable to a CSV format string
            var sb = new StringBuilder();
            IEnumerable<string> columnNames = dataTable.Columns.Cast<DataColumn>().Select(column => column.ColumnName);
            sb.AppendLine(string.Join(",", columnNames));

            foreach (DataRow row in dataTable.Rows)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                sb.AppendLine(string.Join(",", fields));
            }


            // Write the CSV data to a temporary file
            string tempFilePath = Path.GetTempFileName();
            File.WriteAllText(tempFilePath, sb.ToString());
            string unixSyleRScript = rScriptPath.Replace("\\", "/");
            string unixTmpFilePath = tempFilePath.Replace("\\", "/");
            string dataWD = Directory.GetParent(tempFilePath).FullName;


            // Begin process
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = $"--vanilla --slave \"{unixSyleRScript}\" \"{unixTmpFilePath}\"",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = dataWD,
                CreateNoWindow = true
            };

            using (Process process = new Process { StartInfo = processStartInfo })
            {
                process.Start();

                // Read the results of the R commands from the standard output and error
                string output = process.StandardOutput.ReadToEnd();
                string errors = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (!string.IsNullOrEmpty(errors))
                {
                    Console.WriteLine("Error: " + errors);
                }

                File.Delete(tempFilePath);

                return output;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: " + ex.Message);
            return null; // Return null to indicate failure
        }
    }

    private static DataTable ReadExcelFileToDataTable(string filePath)
    {
        // Note: you must install NuGet packages: ExcelDataReader and ExcelDataReader.DataSet

        // Initialize the DataSet and DataTable
        DataSet ds = new DataSet();
        DataTable dtFromExcel = new DataTable();

        try
        {
            // Ensure the file exists
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The file {filePath} was not found.");
            }

            // Open the file stream for the Excel file
            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                // Create and open the Excel file reader
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    // Fetch the content to DataSet with configuration to use the first row as header
                    ds = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = _ => new ExcelDataTableConfiguration()
                        {
                            UseHeaderRow = true // Use first row as header
                        }
                    });

                    // Optional: Select a specific sheet if necessary
                    if (ds.Tables.Count > 0)
                    {
                        dtFromExcel = ds.Tables[0]; // Adjust the index if needed or select by sheet name
                    }
                    else
                    {
                        throw new DataException("No sheets are available to read.");
                    }
                }
            }
        }
        catch (FileNotFoundException fnfEx)
        {
            // Log and rethrow or handle file not found exception
            Console.WriteLine("File not found: " + fnfEx.Message);
            throw; // Rethrowing the exception preserves the original stack trace
        }
        catch (IOException ioEx)
        {
            // Handle IO exceptions if there's a problem accessing the file
            Console.WriteLine("An IO exception occurred: " + ioEx.Message);
            throw;
        }
        catch (Exception ex)
        {
            // General exception handling for other types of exceptions
            Console.WriteLine("An error occurred: " + ex.Message);
            throw;
        }

        return dtFromExcel;
    }
}
