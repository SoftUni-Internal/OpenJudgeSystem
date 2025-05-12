namespace IntegrationTests.Worker.TypeScript;

using OJS.PubSub.Worker.Models.Submissions;
using OJS.Services.Common.Models.Submissions.ExecutionDetails;
using OJS.Workers.Common.Models;

public class TypeScriptCodeMochaUnitsTestsNodeJs20SubmissionFactory : BaseSubmissionFactory<TypeScriptCodeMochaUnitsTestsNodeJs20Parameters>
{
    public override SubmissionForProcessingPubSubModel GetSubmission(TypeScriptCodeMochaUnitsTestsNodeJs20Parameters strategyParameters)
    {
        var submission = new SubmissionForProcessingPubSubModel
        {
            Id = this.GetNextId(),
            ExecutionType = (ExecutionType)2,
            ExecutionStrategy = (ExecutionStrategyType)75,
            CompilerType = (CompilerType)13,
            FileContent = Array.Empty<byte>(),
            AdditionalFiles = Array.Empty<byte>(),
            Code = strategyParameters.Code,
            TimeLimit = 100,
            ExecutionStrategyBaseTimeLimit = null,
            MemoryLimit = 16777216,
            ExecutionStrategyBaseMemoryLimit = null,
            Verbosely = false,
            SimpleExecutionDetails = null,
            TestsExecutionDetails = new TestsExecutionDetailsServiceModel
            {
                MaxPoints = 100,
                TaskId = null,
                CheckerType = "TrimChecker",
                CheckerParameter = null,
                // TaskSkeleton = "K6ksSFUISS0uycxLDwGxbRWKS4qAHIUahbzS3KTUImteLgA=",
                TaskSkeletonAsString = null,
                Tests = new List<TestContext>
                {
                    new TestContext
                    {
                        Id = 387475,
                        Input = @"interface IOven {
                            set temp(val: number);
                            get temp(): number;
                            turnOn(): boolean;
                            turnOff(): boolean;
                            cook(recipe: string, temperature: number): string;
                        }

                        function isOven(potentialOven: any): potentialOven is IOven {
                            let tempDescriptor = Object.getOwnPropertyDescriptor(potentialOven, 'temp');
                            if (!tempDescriptor) {
                                tempDescriptor = Object.getOwnPropertyDescriptor(Object.getPrototypeOf(potentialOven), 'temp');
                            }

                            let hasGetter = tempDescriptor?.get !== undefined;
                            let hasSetter = tempDescriptor?.set !== undefined;

                            return hasSetter && hasGetter &&
                                ""turnOn"" in potentialOven && typeof potentialOven.turnOn === 'function' &&
                                ""turnOff"" in potentialOven && typeof potentialOven.turnOff === 'function' &&
                                ""cook"" in potentialOven && typeof potentialOven.cook === 'function';
                        }

                        const Oven = result;
                        let oven = new Oven(300);
                        if (isOven(oven)) {
                            let temp = 30;
                            expect(() => oven.temp = temp).to.throw('Oven is not turned on', 'Attempting to set temperature before the oven was turned on did not throw.');
                        }",
                        Output = "yes",
                        IsTrialTest = false,
                        OrderBy = 0
                    },
                    new TestContext
                    {
                        Id = 387476,
                        Input = @"//Test set temperature to negative value
                        interface IOven {
                            set temp(val: number);
                            get temp(): number;
                            turnOn(): boolean;
                            turnOff(): boolean;
                            cook(recipe: string, temperature: number): string;
                        }

                        function isOven(potentialOven: any): potentialOven is IOven {
                            let tempDescriptor = Object.getOwnPropertyDescriptor(potentialOven, 'temp');
                            if (!tempDescriptor) {
                                tempDescriptor = Object.getOwnPropertyDescriptor(Object.getPrototypeOf(potentialOven), 'temp');
                            }

                            let hasGetter = tempDescriptor?.get !== undefined;
                            let hasSetter = tempDescriptor?.set !== undefined;

                            return hasSetter && hasGetter &&
                                ""turnOn"" in potentialOven && typeof potentialOven.turnOn === 'function' &&
                                ""turnOff"" in potentialOven && typeof potentialOven.turnOff === 'function' &&
                                ""cook"" in potentialOven && typeof potentialOven.cook === 'function';
                        }

                        const Oven = result;
                        let oven = new Oven(300);
                        if (isOven(oven)) {
                            let temp = -10;
                            oven.turnOn();
                            expect(() => oven.temp = temp).to.throw('Temperature cannot be set bellow 0', 'Setting temperature bellow 0 did not throw');
                        }",
                        Output = "yes",
                        IsTrialTest = false,
                        OrderBy = 0
                    },
                    new TestContext
                    {
                        Id = 387477,
                        Input = @"//Test set temperature over max temperature
                            interface IOven {
                                set temp(val: number);
                                get temp(): number;
                                turnOn(): boolean;
                                turnOff(): boolean;
                                cook(recipe: string, temperature: number): string;
                            }

                            function isOven(potentialOven: any): potentialOven is IOven {
                                let tempDescriptor = Object.getOwnPropertyDescriptor(potentialOven, 'temp');
                                if (!tempDescriptor) {
                                    tempDescriptor = Object.getOwnPropertyDescriptor(Object.getPrototypeOf(potentialOven), 'temp');
                                }

                                let hasGetter = tempDescriptor?.get !== undefined;
                                let hasSetter = tempDescriptor?.set !== undefined;

                                return hasSetter && hasGetter &&
                                    ""turnOn"" in potentialOven && typeof potentialOven.turnOn === 'function' &&
                                    ""turnOff"" in potentialOven && typeof potentialOven.turnOff === 'function' &&
                                    ""cook"" in potentialOven && typeof potentialOven.cook === 'function';
                            }

                            const Oven = result;
                            let oven = new Oven(300);
                            if (isOven(oven)) {
                                let temp = 400;
                                oven.turnOn()
                                expect(() => oven.temp = temp).to.throw('Temperature cannot be set over 300', 'Attempting to set temperature over the max did not throw');
                            }",
                        Output = "yes",
                        IsTrialTest = false,
                        OrderBy = 0
                    },
                    new TestContext
                    {
                        Id = 387478,
                        Input = @"//Test temperature returns correctly set value
                            interface IOven {
                                set temp(val: number);
                                get temp(): number;
                                turnOn(): boolean;
                                turnOff(): boolean;
                                cook(recipe: string, temperature: number): string;
                            }

                            function isOven(potentialOven: any): potentialOven is IOven {
                                let tempDescriptor = Object.getOwnPropertyDescriptor(potentialOven, 'temp');
                                if (!tempDescriptor) {
                                    tempDescriptor = Object.getOwnPropertyDescriptor(Object.getPrototypeOf(potentialOven), 'temp');
                                }

                                let hasGetter = tempDescriptor?.get !== undefined;
                                let hasSetter = tempDescriptor?.set !== undefined;

                                return hasSetter && hasGetter &&
                                    ""turnOn"" in potentialOven && typeof potentialOven.turnOn === 'function' &&
                                    ""turnOff"" in potentialOven && typeof potentialOven.turnOff === 'function' &&
                                    ""cook"" in potentialOven && typeof potentialOven.cook === 'function';
                            }

                            const Oven = result;
                            let oven = new Oven(300);
                            if (isOven(oven)) {
                                let temp = 20;
                                oven.turnOn();
                                oven.temp = temp;
                                expect(oven.temp).to.equal(temp, `Temp did not have the expected value of '${temp}'`)
                            }",
                        Output = "yes",
                        IsTrialTest = false,
                        OrderBy = 0
                    },
                    new TestContext
                    {
                        Id = 387479,
                        Input = @"//Test cook with oven turned off
                            interface IOven {
                                set temp(val: number);
                                get temp(): number;
                                turnOn(): boolean;
                                turnOff(): boolean;
                                cook(recipe: string, temperature: number): string;
                            }

                            function isOven(potentialOven: any): potentialOven is IOven {
                                let tempDescriptor = Object.getOwnPropertyDescriptor(potentialOven, 'temp');
                                if (!tempDescriptor) {
                                    tempDescriptor = Object.getOwnPropertyDescriptor(Object.getPrototypeOf(potentialOven), 'temp');
                                }

                                let hasGetter = tempDescriptor?.get !== undefined;
                                let hasSetter = tempDescriptor?.set !== undefined;

                                return hasSetter && hasGetter &&
                                    ""turnOn"" in potentialOven && typeof potentialOven.turnOn === 'function' &&
                                    ""turnOff"" in potentialOven && typeof potentialOven.turnOff === 'function' &&
                                    ""cook"" in potentialOven && typeof potentialOven.cook === 'function';
                            }

                            const Oven = result;
                            let oven = new Oven(300);
                            if (isOven(oven)) {
                                expect(() => oven.cook('Test', 100)).to.throw('Oven is not turned on', 'Attempting to cook before the oven was turned on did not throw.');
                            }",
                        Output = "yes",
                        IsTrialTest = false,
                        OrderBy = 0
                    },
                    new TestContext
                    {
                        Id = 387480,
                        Input = @"//Test cooking with recipe with temperature bellow 0
                            interface IOven {
                                set temp(val: number);
                                get temp(): number;
                                turnOn(): boolean;
                                turnOff(): boolean;
                                cook(recipe: string, temperature: number): string;
                            }

                            function isOven(potentialOven: any): potentialOven is IOven {
                                let tempDescriptor = Object.getOwnPropertyDescriptor(potentialOven, 'temp');
                                if (!tempDescriptor) {
                                    tempDescriptor = Object.getOwnPropertyDescriptor(Object.getPrototypeOf(potentialOven), 'temp');
                                }

                                let hasGetter = tempDescriptor?.get !== undefined;
                                let hasSetter = tempDescriptor?.set !== undefined;

                                return hasSetter && hasGetter &&
                                    ""turnOn"" in potentialOven && typeof potentialOven.turnOn === 'function' &&
                                    ""turnOff"" in potentialOven && typeof potentialOven.turnOff === 'function' &&
                                    ""cook"" in potentialOven && typeof potentialOven.cook === 'function';
                            }

                            const Oven = result;
                            let oven = new Oven(300);
                            if (isOven(oven)) {
                                let temp = -100;
                                oven.turnOn()
                                expect(() => oven.cook('Test', temp)).to.throw('Temperature cannot be set bellow 0', 'Attempting to cook with a recipe with minus temperature did not throw');
                            }",
                        Output = "yes",
                        IsTrialTest = false,
                        OrderBy = 0
                    },
                    new TestContext
                    {
                        Id = 387481,
                        Input = @"//Test cooking with recipe with temperature above max temperature
                            interface IOven {
                                set temp(val: number);
                                get temp(): number;
                                turnOn(): boolean;
                                turnOff(): boolean;
                                cook(recipe: string, temperature: number): string;
                            }

                            function isOven(potentialOven: any): potentialOven is IOven {
                                let tempDescriptor = Object.getOwnPropertyDescriptor(potentialOven, 'temp');
                                if (!tempDescriptor) {
                                    tempDescriptor = Object.getOwnPropertyDescriptor(Object.getPrototypeOf(potentialOven), 'temp');
                                }

                                let hasGetter = tempDescriptor?.get !== undefined;
                                let hasSetter = tempDescriptor?.set !== undefined;

                                return hasSetter && hasGetter &&
                                    ""turnOn"" in potentialOven && typeof potentialOven.turnOn === 'function' &&
                                    ""turnOff"" in potentialOven && typeof potentialOven.turnOff === 'function' &&
                                    ""cook"" in potentialOven && typeof potentialOven.cook === 'function';
                            }

                            const Oven = result;
                            let oven = new Oven(300);
                            if (isOven(oven)) {
                                let temp = 400;
                                oven.turnOn()
                                expect(() => oven.cook('Test', temp)).to.throw('Temperature cannot be set over 300', 'Attempting to cook with recipe with temperature over the max did not throw');
                            }",
                        Output = "yes",
                        IsTrialTest = false,
                        OrderBy = 0
                    },
                    new TestContext
                    {
                        Id = 387482,
                        Input = @"//Test cooking recipe with valid temperature
                            interface IOven {
                                set temp(val: number);
                                get temp(): number;
                                turnOn(): boolean;
                                turnOff(): boolean;
                                cook(recipe: string, temperature: number): string;
                            }

                            function isOven(potentialOven: any): potentialOven is IOven {
                                let tempDescriptor = Object.getOwnPropertyDescriptor(potentialOven, 'temp');
                                if (!tempDescriptor) {
                                    tempDescriptor = Object.getOwnPropertyDescriptor(Object.getPrototypeOf(potentialOven), 'temp');
                                }

                                let hasGetter = tempDescriptor?.get !== undefined;
                                let hasSetter = tempDescriptor?.set !== undefined;

                                return hasSetter && hasGetter &&
                                    ""turnOn"" in potentialOven && typeof potentialOven.turnOn === 'function' &&
                                    ""turnOff"" in potentialOven && typeof potentialOven.turnOff === 'function' &&
                                    ""cook"" in potentialOven && typeof potentialOven.cook === 'function';
                            }

                            const Oven = result;
                            let oven = new Oven(300);
                            if (isOven(oven)) {
                                let temp = 250;
                                let recipe = 'Fish';
                                oven.turnOn()
                                expect(oven.cook(recipe, temp)).to.equal(`Recipe ${recipe} was cooked successfully`, 'Cooking with valid recipe and temp did not return expected result');
                            }",
                        Output = "yes",
                        IsTrialTest = false,
                        OrderBy = 0
                    }
                }
            }
        };

        return submission;
    }
}