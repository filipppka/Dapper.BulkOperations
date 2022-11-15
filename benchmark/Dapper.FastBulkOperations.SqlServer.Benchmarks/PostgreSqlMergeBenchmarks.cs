using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlClient;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Dapper.FastBulkOperations.SqlServer.Benchmarks;
using Npgsql;
using Z.Dapper.Plus;



namespace Dapper.FastBulkOperations.SqlServer.Benchmarks;
internal class BulkPgMergeTest
{
    public long Id { get; set; }
    public string TestVarchar { get; set; }
    public int TestInt { get; set; }
}

public class TestPks
{
    public Guid FirstKey { get; set; }
	
    public Guid SecondKey { get; set; }
    public string FieldToUpdate { get; set; }
}
[SimpleJob(RunStrategy.Monitoring)]
[AllStatisticsColumn]
[Config(typeof(AntiVirusFriendlyConfig))]
[MemoryDiagnoser(displayGenColumns:true)]
public class PostgreSqlMergeBenchmarks
{
    private readonly List<BulkPgMergeTest> _list = new List<BulkPgMergeTest>();
    [GlobalSetup]
    public void GlobalSetup()
    {
        DapperPlusManager.Entity<BulkPgMergeTest>()
            .Identity(x => x.Id);
        var temp = new List<BulkPgMergeTest>();
        for (var i = 0; i < 50000; i++)
        {
            temp.Add(new BulkPgMergeTest {  TestVarchar = $"test{10 + i}"});
        }
        for (var i = 0; i < 50000; i++)
        {
            _list.Add(new BulkPgMergeTest {  TestVarchar = $"test{10 + i}"});
        }
        using var connection = new NpgsqlConnection("Server=localhost; Port=5432; User Id=postgres; Password=1; Database=tempdb");
        {
            Dapper.FastBulkOperations.PostgreSql.NpgsqlBulkExtensions.BulkInsertOrUpdate(connection, temp);
        }
        _list.AddRange(temp);
        
    }
    
    [Benchmark]
    public void BulkExtensions()
    {
        using var connection =
            new Npgsql.NpgsqlConnection("Server=localhost; Port=5432; User Id=postgres; Password=1; Database=tempdb");
        Dapper.FastBulkOperations.PostgreSql.NpgsqlBulkExtensions.BulkInsertOrUpdate(connection, _list);
    }
    
    [Benchmark]
    public void DapperPlus()
    {
        using var connection =
            new Npgsql.NpgsqlConnection("Server=localhost; Port=5432; User Id=postgres; Password=1; Database=tempdb");
        Z.Dapper.Plus.DapperPlusExtensions.BulkMerge(connection, _list);
    }
}