using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;

namespace EventinatR.Tests.InMemory
{
    internal abstract class MoqDataAttribute : AutoDataAttribute
    {
        public MoqDataAttribute(Action<IFixture> customize)
            : base(() =>
            {
                var fixture = new Fixture();

                fixture.Customize(new AutoMoqCustomization());

                customize(fixture);

                return fixture;
            })
        {
        }
    }
}
