using System.Collections.Generic;

namespace DraggableReorder.Sample.Server.Data
{
    internal class TestDataService
    {
        public IEnumerable<TestData> GetTestData()
        {
            yield return new TestData { Name = "Test 1", Description = "This is a test description 1", Order = 0 };
            yield return new TestData { Name = "Test 2", Description = "This is a test description 2", Order = 1 };
            yield return new TestData { Name = "Test 4", Description = "This is a test description 4", Order = 3 };
            yield return new TestData { Name = "Test 6", Description = "This is a test description 6", Order = 5 };
            yield return new TestData { Name = "Test 11", Description = "This is a test description 11", Order = 10 };
            yield return new TestData { Name = "Test 8", Description = "This is a test description 8", Order = 7 };
            yield return new TestData { Name = "Test 5", Description = "This is a test description 5", Order = 4 };
            yield return new TestData { Name = "Test 3", Description = "This is a test description 3", Order = 2 };
            yield return new TestData { Name = "Test 9", Description = "This is a test description 9", Order = 8 };
            yield return new TestData { Name = "Test 10", Description = "This is a test description 10", Order = 9 };
            yield return new TestData { Name = "Test 7", Description = "This is a test description 7", Order = 6 };
            yield return new TestData { Name = "Test 12", Description = "This is a test description 12", Order = 11 };
            yield return new TestData { Name = "Test 13", Description = "This is a test description 13", Order = 12 };
            yield return new TestData { Name = "Test 20", Description = "This is a test description 20", Order = 19 };
            yield return new TestData { Name = "Test 16", Description = "This is a test description 16", Order = 15 };
            yield return new TestData { Name = "Test 18", Description = "This is a test description 18", Order = 17 };
            yield return new TestData { Name = "Test 19", Description = "This is a test description 19", Order = 18 };
            yield return new TestData { Name = "Test 17", Description = "This is a test description 17", Order = 16 };
            yield return new TestData { Name = "Test 14", Description = "This is a test description 14", Order = 13 };
            yield return new TestData { Name = "Test 15", Description = "This is a test description 15", Order = 14 };
        }
    }
}
