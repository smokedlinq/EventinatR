using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventinatR.InMemory;

namespace EventinatR.Tests.Scenarios.BasicUsage
{
    public class BasicUsageTests
    {
        [Theory, InMemory.InMemoryAutoData]
        public async Task Run(InMemoryEventStore store)
        {
            var favorites = Group.Create("Favorite Final Fantasy Games");

            favorites.AddMember("Final Fantasy VI");
            favorites.AddMember("Final Fantasy VII");
            favorites.AddMember("Final Fantasy X");
            favorites.AddMember("Final Fantasy XV");

            var stream = await store.GetStreamAsync(favorites.Id.Name);

            await favorites.SaveAsync(stream);

            favorites = await Group.ReadAsync(stream);

            favorites.Should().NotBeNull();
            favorites!.Id.Name.Should().Be("Favorite Final Fantasy Games");
            favorites.Members.Should().NotBeNull();
            favorites.Members.Count().Should().Be(4);
        }
    }
}
