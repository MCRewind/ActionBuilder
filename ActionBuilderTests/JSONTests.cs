using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Numerics;
using ActionBuilder;
using FluentAssertions;
using FluentAssertions.Common;
using FluentAssertions.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace ActionBuilderTests
{
    [TestClass]
    public class JsonTests
    {
        [TestMethod]
        public void ActionSerializeDeserializeTest()
        {
            var action = new ActionInfo()
            {
                Name = "Test Action",
                Type = Types.ActionType.JAB,
                Hitboxes = new List<List<ActionInfo.Box>>()
                {
                    // frame 0
                    new List<ActionInfo.Box>()
                    {
                        // hitbox 0
                        new ActionInfo.Box()
                        {
                            X = 0,
                            Y = 0,
                            Width = 4,
                            Height = 4,
                            Damage = 10,
                            KnockbackAngle = new Vector2(0),
                            KnockbackStrength = 5,
                            Lifespan = 1
                        }
                    }
                },
                Hurtboxes = new List<List<ActionInfo.Box>>()
                {
                    // frame 0
                    new List<ActionInfo.Box>()
                    {
                        // hurtbox 0
                        new ActionInfo.Box()
                        {
                            X = 10,
                            Y = 10,
                            Width = 10,
                            Height = 10,
                            Damage = 0,
                            KnockbackAngle = new Vector2(0),
                            KnockbackStrength = 0,
                            Lifespan = 1
                        }
                    }
                }
            };

            var serializedAction = JsonConvert.SerializeObject(action, Formatting.Indented);
            var deserializedAction = JsonConvert.DeserializeObject<ActionInfo>(serializedAction);

            action.Should().Be(deserializedAction);

        }
    }
}