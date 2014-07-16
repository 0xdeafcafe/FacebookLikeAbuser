using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading;
using Facebook;

namespace FacebookLikeAbuser
{
	class Program
	{
		/// <summary>
		///		Your access token. Go to https://developers.facebook.com/tools/explorer/, and copy 
		///		the Access Token in the box at the top. It's only valid for 1 hour though, so keep
		///		that in mind.
		/// </summary>
		private const string AccessToken = "";

		/// <summary>
		/// The username/id of the person to gather likes for and really piss off.
		/// </summary>
		private const string PosterFacebookId = "";

		/// <summary>
		/// The number of pages to go back in each node type. More pages = more likes.
		/// </summary>
		private const int PagesToResearch = 10;

		/// <summary>
		/// Used to generate random sleep intervals. Makes shit more real.
		/// </summary>
		private static readonly Random Random = new Random();

		/// <summary>
		///		Indicates if we should ask the user what to do in the event of them being banned
		///		from liking. If set to false, then just sleep.
		/// </summary>
		private const bool UserVerificationOnLikeBan = false;

		static void Main()
		{
			var fbClient = new FacebookClient(AccessToken);
			var ids = new List<dynamic>();

			foreach (var type in new[] { "feed", "photos", "photos/uploaded", "videos", "videos/uploaded", "albums" })
			{
				var url = String.Format("{0}/{1}?fields=comments.fields(id),id", PosterFacebookId, type);
				for (var i = 0; i < PagesToResearch; i++)
				{
					if (url == null) break;
					dynamic result = fbClient.Get(url);

					if (result.data == null) continue;

					foreach (var post in result.data)
					{
						Console.WriteLine("Added: {0}", post);
						ids.Add(post.id);

						if (post.comments == null || post.comments.data == null) continue;

						foreach (var comment in post.comments.data)
							ids.Add(comment.id);
					}

					try
					{
						url = result.paging.next;
					}
					catch
					{
						break;
					}
				}
			}

			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine("... Now Liking {0} post{1}", ids.Count, ids.Count == 1 ? "" : "s");
			Console.WriteLine();
			Console.WriteLine();

			foreach (var id in ids)
			{
				try
				{
					Console.WriteLine("Liked Post {0}", id);
					fbClient.Post(String.Format("/{0}/likes", id), new ExpandoObject());
					Thread.Sleep(Random.Next(500, 2000));
				}
				catch (FacebookOAuthException ex)
				{
					if (ex.ErrorCode == 100) continue; // Harmless id error
					if (ex.ErrorCode == 190)
					{
						Console.WriteLine("Access Token has Expired...");
						break;
					}
					if (ex.ErrorCode == 368)
					{
						var sleepSeconds = Random.Next(120000, 200000) / 3;

						if (!UserVerificationOnLikeBan)
						{
							Thread.Sleep(sleepSeconds);
							continue;
						}

						Console.WriteLine("Banned from liking. Cancel or Sleep for {0} seconds? (c/s)", sleepSeconds);

						var response = Console.ReadLine();
						if (response == null) break;
						if (response.ToLowerInvariant() == "c") break;

						Thread.Sleep(sleepSeconds);
						break;
					}

					Console.WriteLine("Unknown Exception hit :: {0}", ex);
				}
			}

			Console.WriteLine("Finished xox");
			Console.ReadLine();
		}
	}
}
