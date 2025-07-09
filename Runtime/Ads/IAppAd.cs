using System;


namespace THEBADDEST.Advertisement
{

	public class AdValue
	{
		public AdValue.PrecisionType Precision { get; set; }

		public long Value { get; set; }

		public string CurrencyCode { get; set; }

		public enum PrecisionType
		{
			Unknown,
			Estimated,
			PublisherProvided,
			Precise,
		}
	}

	public interface IAppAd
	{

		//TODO can add more events
		event Action              OnAdLoaded;
		event Action OnAdLoadFailed;
		event Action<AdValue>     OnAdPaid;

		object ad { get; }

		void Create();

		void Destroy();

		bool IsLoaded();
		void Show();

		void Load();

		void Hide();

	}

	public interface IAppRewardAd : IAppAd
	{
		event Action<object> OnRewardClaimed;
		void                 Show(Action<object> onRewardClaimed);

	}


}