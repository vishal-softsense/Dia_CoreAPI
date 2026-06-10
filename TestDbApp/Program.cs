using System;
using System.Data.SqlClient;

class Program {
    static void Main() {
        using(var conn = new SqlConnection("Server=Raj;Database=CompanyMaster;User Id=sa;Password=Soft123;TrustServerCertificate=True;")) {
            conn.Open();
            using(var cmd = conn.CreateCommand()) {
                cmd.CommandText = "EXEC SP_GetUserDataUsingWhere @Where=' AND UserID=1', @ActionType='userdetail'";
                using(var reader = cmd.ExecuteReader()) {
                    for(int i=0; i<reader.FieldCount; i++) {
                        Console.WriteLine(reader.GetName(i));
                    }
                    if (reader.Read()) {
                        Console.WriteLine("\nValues:");
                        for(int i=0; i<reader.FieldCount; i++) {
                            Console.WriteLine($"{reader.GetName(i)}: {reader.GetValue(i)}");
                        }
                    }
                }
            }
        }
    }
}
