using Application.Dtos.Request;
using Application.Services.impl;

using Domain.Exception;

using Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Tests.Services;

[TestFixture]
public class CustomerServiceTest
{

    private CustomerService _customerService;
    private ApplicationDbContext _dbContext;

    [SetUp]
    public void SetUp()
    {
        _dbContext = GetInMemoryDbContext();
        _customerService = new CustomerService(_dbContext, new MemoryCache(new MemoryCacheOptions()));
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
    }

    [Test]
    [TestCaseSource(nameof(CreateCustomerDto))]
    public async Task GetAllCustomers(CustomerDto customerDto)
    {
        await _customerService.Create(customerDto);

        var result = await _customerService.FindAll();

        Assert.That(result, !Is.Empty);
    }

    [Test]
    [TestCaseSource(nameof(CreateCustomerDto))]
    public async Task SearchCustomer_ShouldReturnListOfCustomers(CustomerDto customerDto)
    {
        await _customerService.Create(customerDto);

        var (total, data) = await _customerService.SearchCustomers("na", 0, 10);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(total, Is.GreaterThan(0));
            Assert.That(data, Is.Not.Null);
        }
    }

    [Test]
    [TestCaseSource(nameof(CreateCustomerDto))]
    public async Task CreateSuccess(CustomerDto customerDto)
    {
        var result = await _customerService.Create(customerDto);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Id, Is.GreaterThan(0));
        }
    }

    [TestCase("", "e@mail.com", "123", "addr", "Name")]
    [TestCase("Name", "", "123", "addr", "Email")]
    [TestCase("Name", "e@mail.com", "", "addr", "Phone")]
    [TestCase("Name", "e@mail.com", "123", "", "Address")]
    public Task Create_ShouldThrowValidationException(
        string name,
        string email,
        string phone,
        string address,
        string expectedKey)
    {
        var ex = Assert.ThrowsAsync<ValidationException>(
            async () => await _customerService.Create(new CustomerDto(name, email, phone, address))
        );

        using (Assert.EnterMultipleScope())
        {
            Assert.That(ex.Errors.ContainsKey(expectedKey), Is.True);
            Assert.That(ex.Errors[expectedKey], Does.Not.Empty);
        }
        return Task.CompletedTask;
    }

    [Test]
    public async Task Update()
    {
        await _customerService.Create(new CustomerDto("Name", "Email@gmail.com", "843806784", "1b22"));

        var result = await _customerService.Update(1, new CustomerDto("Name13", "Emai44l@gmail.com", "843806784", "1b22"));

        Assert.That(result, Is.EqualTo("Customer updated successfully"));
    }

    [Test]
    [TestCase("", "e@mail.com", "123", "addr")]
    [TestCase("Name", "", "123", "addr")]
    [TestCase("Name", "e@mail.com", "", "addr")]
    [TestCase("Name", "e@mail.com", "123", "")]
    public Task Update_ShouldThrowNotFoundException(string name, string email, string phone, string address)
    {
        var ex = Assert.ThrowsAsync<NotFoundException>(async () =>
            await _customerService.Update(1, new CustomerDto(name, email, phone, address)));

        Assert.That(ex.Message, Is.EqualTo($"Customer with id: 1 not found"));

        return Task.CompletedTask;
    }

    [Test]
    [TestCaseSource(nameof(CreateCustomerDto))]
    public async Task FindById(CustomerDto customerDto)
    {
        var customer = await _customerService.Create(customerDto);

        var result = await _customerService.FindById(customer.Id);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Id, Is.Not.Zero);
            Assert.That(result.Name, Is.EqualTo(customer.Name));
            Assert.That(result.Email, Is.EqualTo(customer.Email));
            Assert.That(result.Phone, Is.EqualTo(customer.Phone));
            Assert.That(result.Address, Is.EqualTo(customer.Address));
        }
    }

    [Test]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    public Task FindById_ShouldThrowNotFoundException(int id)
    {
        var ex = Assert.ThrowsAsync<NotFoundException>(async () =>
            await _customerService.FindById(id));
        Assert.That(ex.Message, Is.EqualTo($"Customer with id: {id} not found"));
        return Task.CompletedTask;
    }

    [Test]
    [TestCaseSource(nameof(CreateCustomerDto))]
    public async Task FindByName(CustomerDto customerDto)
    {
        var customer = await _customerService.Create(customerDto);

        var result = await _customerService.FindByName(customer.Name);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Id, Is.Not.Zero);
            Assert.That(result.Name, Is.EqualTo(customer.Name));
            Assert.That(result.Email, Is.EqualTo(customer.Email));
            Assert.That(result.Phone, Is.EqualTo(customer.Phone));
            Assert.That(result.Address, Is.EqualTo(customer.Address));
        }
    }

    [Test]
    [TestCase("we")]
    [TestCase("zx")]
    [TestCase("cd")]
    [TestCase("ff")]
    public Task FindByName_ShouldThrowNotFoundException(string name)
    {
        var ex = Assert.ThrowsAsync<NotFoundException>(async () =>
            await _customerService.FindByName(name));
        Assert.That(ex.Message, Is.EqualTo($"Customer with name: {name} not found"));
        return Task.CompletedTask;
    }

    [Test]
    [TestCaseSource(nameof(CreateCustomerDto))]
    public async Task FindByEmail(CustomerDto customerDto)
    {
        var customer = await _customerService.Create(customerDto);

        var result = await _customerService.FindByEmail(customer.Email);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Id, Is.Not.Zero);
            Assert.That(result.Name, Is.EqualTo(customer.Name));
            Assert.That(result.Email, Is.EqualTo(customer.Email));
            Assert.That(result.Phone, Is.EqualTo(customer.Phone));
            Assert.That(result.Address, Is.EqualTo(customer.Address));
        }
    }

    [Test]
    [TestCase("we@gmail.com")]
    [TestCase("zx@gmail.com")]
    [TestCase("cd@gmail.com")]
    [TestCase("ff@gmail.com")]
    public Task FindByEmail_ShouldThrowNotFoundException(string email)
    {
        var ex = Assert.ThrowsAsync<NotFoundException>(async () =>
            await _customerService.FindByEmail(email));
        Assert.That(ex.Message, Is.EqualTo($"Customer with email: {email} not found"));
        return Task.CompletedTask;
    }

    [Test]
    [TestCaseSource(nameof(CreateCustomerDto))]
    public async Task FindByPhoneNumber(CustomerDto customerDto)
    {
        var customer = await _customerService.Create(customerDto);

        var result = await _customerService.FindByPhoneNumber(customer.Phone);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Id, Is.Not.Zero);
            Assert.That(result.Name, Is.EqualTo(customer.Name));
            Assert.That(result.Email, Is.EqualTo(customer.Email));
            Assert.That(result.Phone, Is.EqualTo(customer.Phone));
            Assert.That(result.Address, Is.EqualTo(customer.Address));
        }
    }

    [Test]
    [TestCase("123")]
    [TestCase("456")]
    [TestCase("789")]
    public Task FindByPhone_ShouldThrowNotFoundException(string phone)
    {
        var ex = Assert.ThrowsAsync<NotFoundException>(async () =>
            await _customerService.FindByPhoneNumber(phone));
        Assert.That(ex.Message, Is.EqualTo($"Customer with number: {phone} not found"));
        return Task.CompletedTask;
    }

    [Test]
    [TestCaseSource(nameof(CreateCustomerDto))]
    public async Task DeleteById(CustomerDto customerDto)
    {
        var customer = await _customerService.Create(customerDto);

        var result = await _customerService.DeleteById(customer.Id);

        Assert.That(result, Is.EqualTo("Customer deleted successfully"));
    }

    [Test]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    public Task DeleteById_ShouldThrowNotFoundException(int id)
    {
        var ex = Assert.ThrowsAsync<NotFoundException>(async () =>
            await _customerService.DeleteById(id));
        Assert.That(ex.Message, Is.EqualTo($"Customer with id: {id} not found"));
        return Task.CompletedTask;
    }

    private static IEnumerable<CustomerDto> CreateCustomerDto()
    {
        yield return new CustomerDto("Nam1e", "Email1@gmail.com", "843806784", "1b23");
        yield return new CustomerDto("Nam2e", "Email2@gmail.com", "843806894", "1b24");
        yield return new CustomerDto("Name3", "Email3@gmail.com", "843806554", "1b25");
        yield return new CustomerDto("Nam5e", "Email4@gmail.com", "843806424", "1b26");
        yield return new CustomerDto("Name6", "Email5@gmail.com", "843806324", "1b27");
    }

    private static ApplicationDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
