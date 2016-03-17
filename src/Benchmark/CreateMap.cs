using System;
using AutoMapper;

namespace Benchmark
{
	class CreateMap : IBenchmarker
	{
		private int _fieldMappingsToAdd = 125;

		public string Name => nameof(CreateMap);

		public int Iterations => 5;

		public void Initialize() { }

		public void Execute()
		{
			MapperConfiguration config = new MapperConfiguration(cfg =>
			{
				cfg.ReplaceMemberName("_", "");
				for (int n = 0; n < _fieldMappingsToAdd; n++)
				{
					cfg.RecognizeAlias("abc" + n, "xyz" + n);
				}
				cfg.CreateMap<SourceModel, DestinationModel>();
			});
			var mapper = config.CreateMapper();
		}
	}

	class SourceModel
	{
		public string Field_00;
		public string Field_01;
		public string Field_02;
		public string Field_03;
		public string Field_04;
		public string Field_05;
		public string Field_06;
		public string Field_07;
		public string Field_08;
		public string Field_09;
		public string Field_10;
		public string Field_11;
		public string Field_12;
		public string Field_13;
		public string Field_14;
		public string Field_15;
		public string Field_16;
		public string Field_17;
		public string Field_18;
		public string Field_19;
		public string Field_20;
		public string Field_21;
		public string Field_22;
		public string Field_23;
		public string Field_24;
		public string Field_25;
		public string Field_26;
		public string Field_27;
		public string Field_28;
		public string Field_29;
	}

	class DestinationModel
	{
		public string field00;
		public string field01;
		public string field02;
		public string field03;
		public string field04;
		public string field05;
		public string field06;
		public string field07;
		public string field08;
		public string field09;
		public string field10;
		public string field11;
		public string field12;
		public string field13;
		public string field14;
		public string field15;
		public string field16;
		public string field17;
		public string field18;
		public string field19;

		public string rename20;
		public string rename21;
		public string rename22;
		public string rename23;
		public string rename24;
		public string rename25;
		public string rename26;
		public string rename27;
		public string rename28;
		public string rename29;
	}
}
