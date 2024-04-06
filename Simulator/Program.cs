using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using System.Text;
using System.Text.Json;

namespace Simulator
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("*** Posting Random Data ***");
            await PostData();
            Console.WriteLine("*** Reading Data ***");
            await GetData();
            Console.WriteLine("--------------------");
        }
        static async Task PostData()
        {
            List<Customer> customers = await CreateCustomers();

            Random random = new Random();
            int totalRecords = customers.Count;

            Dictionary<int, List<Customer>> parallelReq = new Dictionary<int, List<Customer>>();

            int i = 1;
            while (totalRecords > 0)
            {
                int randomCount = random.Next(2, totalRecords);
                int reqCount = (randomCount % 2 > 0) ? randomCount + 1 : randomCount;

                parallelReq.Add(i++, customers.Where(c => c.Id >= i).Take(reqCount).ToList());

                totalRecords = totalRecords - reqCount;
            }

            Dictionary<int, bool> postedPairs = new Dictionary<int, bool>();

            var tasks = parallelReq.Select(i => PostToRest(i.Value));
            await Task.WhenAll(tasks);

        }
        static async Task GetData()
        {
            List<Customer> customers = await GetCustomersFromRest();
            foreach(Customer customer in customers)
            {
                Console.WriteLine("{0} - {1} {2} - {3}", customer.Id, customer.FirstName, customer.LastName, customer.Age);
            }
        }
        static async Task<List<Customer>> GetCustomersFromRest()
        {
            List<Customer> customers = new List<Customer>();
            try
            {
                HttpClient httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri("http://localhost:5070/");
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(
                                new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await httpClient.GetAsync("api/customer");


                if (response.IsSuccessStatusCode)
                {
                    customers = JsonSerializer.Deserialize<List<Customer>>(response.Content.ReadAsStringAsync().Result, new JsonSerializerOptions { PropertyNameCaseInsensitive=true});
                    //Departmentdepartment = awaitresponse.Content.ReadAsAsync<Department>();
                    //Console.WriteLine("Id:{0}\tName:{1}", department.DepartmentId, department.DepartmentName);
                    //Console.WriteLine("No of Employee in Department: {0}", department.Employees.Count);
                }
                else
                {

                    Console.WriteLine("Internal server Error");
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
            }
            return customers;
        }

        static async Task<bool> PostToRest(List<Customer> customers)
        {
            bool isSuccess = false;
            try
            {
                HttpClient httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri("http://localhost:5070/");
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(
                                new MediaTypeWithQualityHeaderValue("application/json"));

                var content = new StringContent(JsonSerializer.Serialize(customers), Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("api/customer", content);


                if (response.IsSuccessStatusCode)
                {
                    isSuccess = true;
                    //Departmentdepartment = awaitresponse.Content.ReadAsAsync<Department>();
                    //Console.WriteLine("Id:{0}\tName:{1}", department.DepartmentId, department.DepartmentName);
                    //Console.WriteLine("No of Employee in Department: {0}", department.Employees.Count);
                }
                else
                {

                    Console.WriteLine("Internal server Error");
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
            }
            return isSuccess;
        }
        static async Task<List<Customer>> CreateCustomers()
        {
            List<Customer> list = new List<Customer>();
            string[] lastNames = new string[] {
                "Leia",
                "Sadie",
                "Jose",
                "Sara",
                "Frank",
                "Dewey",
                "Tomas",
                "Joel",
                "Lukas",
                "Carlos"
            };
            string[] firstNames = new string[] {
                "Liberty",
                "Ray",
                "Harrison",
                "Ronan",
                "Drew",
                "Powell",
                "Larsen",
                "Chan",
                "Anderson",
                "Lane"};

            Random random = new Random();
            //int age = random.Next(10, 90);
            //int request = random.Next(2, 10);
            int seq = 1;
            for (int i = 0; i < lastNames.Length; i++)
            {
                for (int j = 0; j < firstNames.Length; j++)
                {
                    list.Add(new Customer()
                    {
                        Id = seq++,
                        LastName = lastNames[i],
                        FirstName = firstNames[j],
                        Age = random.Next(18, 90)
                    });
                }
            }

            return list;
        }
    }
}
