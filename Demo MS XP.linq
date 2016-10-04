<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\System.Net.Http.dll</Reference>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>Newtonsoft.Json</Namespace>
</Query>

const string baseUrl = "http://localhost:8080";
const string CsvPath = @"D:\Projets\Demo\catalog.csv";

async Task Main()
{
	var catalog = GetCatalog();

	catalog.Dump();

	await InitCategories();
	await CheckUploadCategories();
	
	await InitProducts(); 
	await CheckUploadProducts(); 
	
	await InitAssociations();
}



async Task InitCategories()
{
	HttpClient client = new HttpClient();
	int success = 0;
	int error = 0;
	foreach (var category in GetCatalog().Categories)
	{
		try
		{
			var response = await client.PostAsync(baseUrl + "/api/catalog/categories", new StringContent(JsonConvert.SerializeObject(category), Encoding.UTF8, "application/json"));
			response.EnsureSuccessStatusCode();

			success++;
		}
		catch
		{
			error++;
		}
	}

	success.Dump("success categories");
	error.Dump("error categories");
}

async Task CheckUploadCategories()
{
	HttpClient client = new HttpClient();
	bool success = false;

	try
	{
		var response = await client.GetStringAsync(baseUrl + "/api/catalog/categories/count");
		int responseAsInt;

		response.Dump();
		if (int.TryParse(response, out responseAsInt))
		{
			if (responseAsInt == GetCatalog().Categories.Count)
			{
				success = true;
			}
		}
	}
	catch
	{
		"Error when call count categories".Dump();
	}

	success.Dump("count categories");
}

async Task InitProducts()
{
	HttpClient client = new HttpClient();
	int success = 0;
	int error = 0;
	foreach (var product in GetCatalog().Products)
	{
		try
		{
			var response = await client.PostAsync(baseUrl + "/api/catalog/products", new StringContent(JsonConvert.SerializeObject(product), Encoding.UTF8, "application/json"));

			response.EnsureSuccessStatusCode();

			success++;
		}
		catch
		{
			error++;
		}
	}

	success.Dump("success products");
	error.Dump("error products");
}

async Task CheckUploadProducts()
{
	HttpClient client = new HttpClient();
	bool success = false;

	try
	{
		var response = await client.GetStringAsync(baseUrl + "/api/catalog/products/count");
		int responseAsInt;

		response.Dump();
		if (int.TryParse(response, out responseAsInt))
		{
			if (responseAsInt == GetCatalog().Products.Count)
			{
				success = true;
			}
		}
	}
	catch
	{
		"Error when call count products".Dump();
	}

	success.Dump("count products");
}


async Task InitAssociations()
{
	HttpClient client = new HttpClient();
	int success = 0;
	int error = 0;
	foreach (var assocations in GetCatalog().Associations)
	{
		try
		{
			var response = await client.PutAsync(baseUrl + "/api/catalog/categories/" + assocations.Item1 + "/products/" + assocations.Item2, new StringContent("", Encoding.UTF8, "application/json"));
			response.EnsureSuccessStatusCode();
			success++;
		}
		catch
		{
			error++;
		}
	}

	success.Dump("success associations");
	error.Dump("error associations");

}



public Catalog GetCatalog()
{
	var cache = Util.Cache<Catalog>(() =>
	{
		"Generate catalog".Dump();
		string path = CsvPath;

		List<Category> categories = new List<Category>();
		List<Product> products = new List<Product>();
		List<Tuple<Guid, Guid>> associations = new List<Tuple<Guid, Guid>>();

		var lines = File.ReadAllLines(path, Encoding.UTF8);

		foreach (var line in lines)
		{
			string[] items = line.Split(new[] { ';' });
			var category = new Category();

			if (!categories.Any(n => n.Name == items[0]))
			{
				category.Id = Guid.NewGuid();
				category.Description = items[0];
				category.Name = items[0];

				categories.Add(category);
			}
			else
			{
				category.Id = categories.First(n => n.Name == items[0]).Id;
			}

			Product product = new Product();
			product.Id = Guid.NewGuid();
			product.Title = items[1];
			product.Price = float.Parse(items[2]);

			for (int i = 3; i < items.Length; i++)
			{
				product.Description += items[i].TrimEnd() + ",";
			}
			product.Description = product.Description.Substring(0, product.Description.Length - 2);
			products.Add(product);

			associations.Add(new Tuple<Guid, Guid>(category.Id, product.Id));
		}

		Catalog catalog = new Catalog();
		catalog.Categories = categories;
		catalog.Products = products;
		catalog.Associations = associations;

		return catalog;
	});

	return cache;
}

[Serializable]
public class Catalog
{
	public List<Category> Categories = new List<Category>();
	public List<Product> Products = new List<Product>();
	public List<Tuple<Guid, Guid>> Associations = new List<Tuple<Guid, Guid>>();
}


[Serializable]
public class Category
{
	public Guid Id { get; set; }
	public string Name { get; set; }
	public string Description { get; set; }
}


[Serializable]
public class Product
{
	public Guid Id { get; set; }
	public string Title { get; set; }
	public string Description { get; set; }
	public float Price { get; set; }
}