using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace ProductCrud
{
    public class SQLClass
    {

        public SQLClass(string query, bool write)
        {

            string connectionString = @"Server=.\SQLEXPRESS;Database=BirthdaySystem;Trusted_Connection=true";

            connectionSQL = new SqlConnection(connectionString);

            try
            {
                //SETUP connection to DB
                connectionSQL.Open();

                command = new SqlCommand(query, connectionSQL);

                if (write == true)
                {
                    Write();
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public List<string> Read()
        {
            List<string> output = new List<string>();
            SqlDataReader reader = command.ExecuteReader();
            try
            {
                while (reader.Read())
                {

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        output.Add(reader.GetValue(i).ToString());
                    }
                }
            }
            finally
            {
                reader.Close();
                Destroy();
            }

            return output;
        }

        private void Write()
        {
            SqlDataAdapter adapter = new SqlDataAdapter();
            try
            {
                //attempt to run
                adapter.InsertCommand = command;
                adapter.InsertCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                adapter.Dispose();
                Destroy();
            }



        }

        private void Destroy()
        {

            //cleanup
            command.Dispose();
            connectionSQL.Dispose();

        }

        private SqlConnection connectionSQL;
        private SqlCommand command;
    }
}
