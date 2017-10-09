# Flattening

One of the common usages of object-object mapping is to take a complex object model and flatten it to a simpler model.  You can take a complex model such as:

```c#
public class Order
{
	private readonly IList<OrderLineItem> _orderLineItems = new List<OrderLineItem>();

	public Customer Customer { get; set; }

	public OrderLineItem[] GetOrderLineItems()
	{
		return _orderLineItems.ToArray();
	}

	public void AddOrderLineItem(Product product, int quantity)
	{
		_orderLineItems.Add(new OrderLineItem(product, quantity));
	}

	public decimal GetTotal()
	{
		return _orderLineItems.Sum(li => li.GetTotal());
	}
}

public class Product
{
	public decimal Price { get; set; }
	public string Name { get; set; }
}

public class OrderLineItem
{
	public OrderLineItem(Product product, int quantity)
	{
		Product = product;
		Quantity = quantity;
	}

	public Product Product { get; private set; }
	public int Quantity { get; private set;}

	public decimal GetTotal()
	{
		return Quantity*Product.Price;
	}
}

public class Customer
{
	public string Name { get; set; }
}
```

We want to flatten this complex Order object into a simpler OrderDto that contains only the data needed for a certain scenario:

```c#
public class OrderDto
{
	public string CustomerName { get; set; }
	public decimal Total { get; set; }
}
```

When you configure a source/destination type pair in AutoMapper, the configurator attempts to match properties and methods on the source type to properties on the destination type.  If for any property on the destination type a property, method, or a method prefixed with "Get" does not exist on the source type, AutoMapper splits the destination member name into individual words (by PascalCase conventions).

```c#
// Complex model

var customer = new Customer
	{
		Name = "George Costanza"
	};
var order = new Order
	{
		Customer = customer
	};
var bosco = new Product
	{
		Name = "Bosco",
		Price = 4.99m
	};
order.AddOrderLineItem(bosco, 15);

// Configure AutoMapper

Mapper.Initialize(cfg => cfg.CreateMap<Order, OrderDto>());

// Perform mapping

OrderDto dto = Mapper.Map<Order, OrderDto>(order);

dto.CustomerName.ShouldEqual("George Costanza");
dto.Total.ShouldEqual(74.85m);
```

We configured the type map in AutoMapper with the CreateMap method.  AutoMapper can only map type pairs it knows about, so we have explicitly register the source/destination type pair with CreateMap.  To perform the mapping, we use the Map method.

On the OrderDto type, the Total property matched to the GetTotal() method on Order.  The CustomerName property matched to the Customer.Name property on Order.  As long as we name our destination properties appropriately, we do not need to configure individual property matching.
