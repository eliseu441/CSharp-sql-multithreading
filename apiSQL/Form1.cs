using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace apiSQL
{

    public partial class Form1 : Form
    {
        string queryString = "SELECT TOP (1000) [ID],[ITEM],[QUANTITY],[PRICE],[ENTRANCE],[ID_PROJECT] " +
            "FROM [HEFESTO].[dbo].[TBF_GENERAL_STOCK]";
        string connectionString = @"Data Source=163.123.183.80,18501;Database=HEFESTO;
                User ID=eliseu441;Password=Trembolona550";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        public static void PrintValues(IEnumerable myCollection, char mySeparator)
        {
            foreach (Object obj in myCollection)
                Console.Write("{0}{1}", mySeparator, obj);
            Console.WriteLine();
        }
        public void callQuery()
        {

            try
            {
                SqlConnection cnn;
            cnn = new SqlConnection(connectionString);
            cnn.Open();
            SqlCommand command = new SqlCommand(queryString, cnn);
            SqlDataReader reader = command.ExecuteReader();

            Stack myStack = new Stack();
            while (reader.Read())
            {
                Console.WriteLine("{0}, {1}, {2}, {3}, {4}, {5}",
                    reader["ID"],
                    reader["ITEM"],
                    reader["QUANTITY"],
                    reader["PRICE"],
                    reader["ENTRANCE"],
                    reader["ID_PROJECT"]);

                    string item = (string)reader["ITEM"];
                myStack.Push(item);
                //MessageBox.Show(teste);
            }



            Console.Write("Stack values:");
            PrintValues(myStack, '\t');
            cnn.Close();
            }
            catch (SqlException erro)
            {
                MessageBox.Show("Erro ao se conectar no banco de dados \n" +
                "Verifique os dados informados" + erro);
            }
        }

        private void btnTestar_Click(object sender, EventArgs e)
        {

            //callQuery(); 
            try
            {
                Random random = new Random();
                int maxTasks = 30,
                    maxActive = 12,
                    maxDelayMs = 1000,
                    currentDelay = -1;
                List<TimeSpan> taskDelays = new List<TimeSpan>(maxTasks);

                for (int i = 0; i < maxTasks; i++)
                {
                    taskDelays.Add(TimeSpan.FromMilliseconds(random.Next(maxDelayMs)));
                }

                Task[] tasks = new Task[maxActive];
                object o = new object();

                for (int i = 0; i < maxActive; i++)
                {
                    int queryIndex = i;

                    tasks[i] = Task.Run(() =>
                    {
                        DelayConsumer(ref currentDelay, taskDelays, o, queryIndex);
                    });
                }

                Console.WriteLine("waiting requisition");

                Task.WaitAll(tasks);

                Console.WriteLine("all requisitions completed!");
            }
            catch (global::System.Exception)
            {

                throw;
            }
        }
        private void DelayConsumer(ref int currentDelay, List<TimeSpan> taskDelays, object o, int queryIndex)
        {
            Console.WriteLine($"query #{queryIndex} starting");

            while (true)
            {
                TimeSpan delay;
                int delayIndex;

                lock (o)
                {
                    delayIndex = ++currentDelay;
                    if (delayIndex < taskDelays.Count)
                    {
                        callQuery();
                        delay = taskDelays[delayIndex];
                    }
                    else
                    {
                        Console.WriteLine($"thread #{queryIndex} exiting");
                        return;
                    }
                }

                Console.WriteLine($"thread #{queryIndex} sleeping for {delay.TotalMilliseconds} ms, task #{delayIndex}");
                System.Threading.Thread.Sleep(delay);
            }
        }
    }
}
