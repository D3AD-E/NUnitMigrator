using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NUnitMigrator.Core.Rewriter
{
    //Taken from legacy code
    public class RewriterData
    {
        

        //attributes
        public class NUnitData
        {
            public static Regex AuthorRegex = new Regex(@"Author.*");
            public static Regex CategoryRegex = new Regex(@"Category.*");
            public static Regex DescriptionRegex = new Regex(@"Description.*");
            public static Regex ExcludePlatformRegex = new Regex(@"ExcludePlatform.*");
            public static Regex ExpectedResultRegex = new Regex(@"ExpectedResult.*");
            public static Regex ExplicitRegex = new Regex(@"Explicit.*");
            public static Regex IgnoreRegex = new Regex(@"Ignore.*");
            public static Regex IgnoreReasonRegex = new Regex(@"IgnoreReason.*");
            public static Regex IncludePlatformRegex = new Regex(@"IncludePlatform.*");
            public static Regex ReasonRegex = new Regex(@"Reason.*");
            public static Regex TestNameRegex = new Regex(@"TestName.*");
            public static Regex TestOfRegex = new Regex(@"TestOf.*");

            public const string APARTMENT_ATTRIBUTE = "Apartment";
            public const string AUTHOR_ATTRIBUTE = "Author";
            public const string CATEGORY_ATTRIBUTE = "Category";
            public const string COMBINATORIAL_ATTRIBUTE = "Combinatorial";
            public const string CULTURE_ATTRIBUTE = "Culture";
            public const string DATAPOINT_ATTRIBUTE = "Datapoint";
            public const string DATAPOINT_SOURCE_ATTRIBUTE = "DatapointSource";
            public const string DFPT_ATTRIBUTE = "DefaultFloatingPointTolerance";
            public const string DESCRIPTION_ATTRIBUTE = "Description";
            public const string EXPLICIT_ATTRIBUTE = "Explicit";
            public const string IGNORE_ATTRIBUTE = "Ignore";
            public const string LOP_ATTRIBUTE = "LevelOfParallelism";
            public const string MAX_TIME_ATTRIBUTE = "MaxTime";
            public const string NON_PARALLELIZABLE_ATTRIBUTE = "NonParallelizable";
            public const string NTA_ATTRIBUTE = "NonTestAssembly";
            public const string OTSU_ATTRIBUTE = "OneTimeSetUp";
            public const string OTTD_ATTRIBUTE = "OneTimeTearDown";
            public const string ORDER_ATTRIBUTE = "Order";
            public const string PAIRWISE_ATTRIBUTE = "Pairwise";
            public const string PARALLELIZABLE_ATTRIBUTE = "Parallelizable";
            public const string PLATFORM_ATTRIBUTE = "Platform";
            public const string PROPERTY_ATTRIBUTE = "Property";
            public const string RANDOM_ATTRIBUTE = "Random";
            public const string RANGE_ATTRIBUTE = "Range";
            public const string REPEAT_ATTRIBUTE = "Repeat";
            public const string REQUIRES_THREAD_ATTRIBUTE = "RequiresThread";
            public const string RETRY_ATTRIBUTE = "Retry";
            public const string SEQUENTIAL_ATTRIBUTE = "Sequential";
            public const string SET_CULTURE_ATTRIBUTE = "SetCulture";
            public const string SET_UI_CULTURE_ATTRIBUTE = "SetUICulture";
            public const string SETUP_ATTRIBUTE = "SetUp";
            public const string SETUP_FIXTURE_ATTRIBUTE = "SetUpFixture";
            public const string SINGLE_THREADED_ATTRIBUTE = "SingleThreaded";
            public const string TEAR_DOWN_ATTRIBUTE = "TearDown";
            public const string TEST_ATTRIBUTE = "Test";
            public const string TEST_CASE_ATTRIBUTE = "TestCase";
            public const string TCS_ATTRIBUTE = "TestCaseSource";
            public const string TEST_FIXTURE_ATTRIBUTE = "TestFixture";
            public const string TFSE_ATTRIBUTE = "TestFixtureSetup";
            public const string TFSO_ATTRIBUTE = "TestFixtureSource";
            public const string TFT_ATTRIBUTE = "TestFixtureTeardown";
            public const string TEST_OF_ATTRIBUTE = "TestOf";
            public const string THEORY_ATTRIBUTE = "Theory";
            public const string TIMEOUT_ATTRIBUTE = "Timeout";
            public const string VALUES_ATTRIBUTE = "Values";
            public const string VALUES_SOURCE_ATTRIBUTE = "ValueSource";
        }

        //attributes MsTest
        public class MSTestData
        {
            public const string TEST_CLASS_ATTRIBUTE = "TestClass";
            public const string DESCRIPTION_ATTRIBUTE = "Description";
            public const string TEST_METHOD_ATTRIBUTE = "TestMethod";
            public const string TEST_CATEGORY_ATTRIBUTE = "TestCategory";
            public const string IGNORE_ATTRIBUTE = "Ignore";
            public const string ASSERT_ATTRIBUTE = "Assert";
            public const string DO_NOT_PARRELELIZE_ATTRIBUTE = "DoNotParallelize";
            public const string TEST_PROPETY_ATTRIBUTE = "TestProperty";
            public const string TEST_INIT_ATTRIBUTE = "TestInitialize";
            public const string TEST_CLEANUP_ATTRIBUTE = "TestCleanup";
            public const string OWNER_ATTRIBUTE = "Owner";
            public const string PARALELIZM_ATTRIBUTE = "LevelOfParallelism";
            public const string CLASS_INIT_ATTRIBUTE = "ClassInitialize";
            public const string SEQ_ATTRIBUTE = "Sequential";
            public const string CLASS_CLEANUP_ATTRIBUTE = "ClassCleanup";
            public const string DATA_ROW_ATTRIBUTE = "DataRow";
            public const string DATA_DYNAMIC_ATTRIBUTE = "DynamicData";
            public const string ONE_SETUP_ATTRIBUTE = "OneTimeSetUp";
            public const string ONE_TEARDOWN_ATTRIBUTE = "OneTimeTearDown";
            public const string TIMEOUT_ATTRIBUTE = "Timeout";
            public const string DATA_SOURCE_ATTRIBUTE = "DataSource";
        }
    }
}
