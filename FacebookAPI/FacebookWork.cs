using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Facebook;
using System.Threading.Tasks;
using System.IO;

namespace FacebookAPI
{
    class FacebookWork
    {
        readonly string _accessToken;
        readonly string _pageID;
        readonly string _facebookAPI = "https://graph.facebook.com/";
        readonly string _pageEdgeFeed = "feed";
        readonly string _pageEdgePhotos = "photos";
        readonly string _postToPageURL;
        readonly string _postToPagePhotosURL;

        public FacebookWork(string accessToken, string pageID)
        {
            _accessToken = accessToken;
            _pageID = pageID;
            _postToPageURL = $"{_facebookAPI}{pageID}/{_pageEdgeFeed}";
            _postToPagePhotosURL = $"{_facebookAPI}{pageID}/{_pageEdgePhotos}";
        }

        /// <summary>
        /// Publish a simple text post
        /// </summary>
        /// <returns>StatusCode and JSON response</returns>
        /// <param name="postText">Text for posting</param>
        public async Task<Tuple<int, string>> PublishSimplePost(string postText)
        {
            using (var http = new HttpClient())
            {
                var postData = new Dictionary<string, string> {
                { "access_token", _accessToken },
                { "message", postText }//,
                // { "formatting", "MARKDOWN" } // doesn't work
            };

                var httpResponse = await http.PostAsync(
                    _postToPageURL,
                    new FormUrlEncodedContent(postData)
                    );
                var httpContent = await httpResponse.Content.ReadAsStringAsync();

                return new Tuple<int, string>(
                    (int)httpResponse.StatusCode,
                    httpContent
                    );
            }
        }

        /// <summary>
        /// Publish a post to Facebook page
        /// </summary>
        /// <returns>Result</returns>
        /// <param name="postText">Post to publish</param>
        /// <param name="pictureURL">Post to publish</param>
        public async Task<Tuple<int, string>> PublishToFacebook(string postText, string pictureURL)
        {
                // upload picture first
                var rezImage = Task.Run(async () =>
                {
                    using (var http = new HttpClient())
                    {
                        return await UploadPhoto(pictureURL);
                    }
                });
                var rezImageJson = JObject.Parse(rezImage.Result.Item2);

                if (rezImage.Result.Item1 != 200)
                {
                    return new Tuple<int, string>(400, null);
                }
                // get post ID from the response
                string postID = rezImageJson["post_id"].Value<string>();

                // and update this post (which is actually a photo) with your text
                var rezText = Task.Run(async () =>
                {
                    using (var http = new HttpClient())
                    {
                        return await UpdatePhotoWithPost(postID, postText);
                    }
                });
                var rezTextJson = JObject.Parse(rezText.Result.Item2);

                if (rezText.Result.Item1 != 200)
                {
                    return new Tuple<int, string>(400, null);
                }

                return new Tuple<int, string>(rezText.Result.Item1, postID);
        }

        /// <summary>
        /// Upload a picture (photo)
        /// </summary>
        /// <returns>StatusCode and JSON response</returns>
        /// <param name="photoURL">URL of the picture to upload</param>
        public async Task<Tuple<int, string>> UploadPhoto(string photoURL)
        {
            using (var http = new HttpClient())
            {
                var postData = new Dictionary<string, string> {
                { "access_token", _accessToken },
                { "url", photoURL }
            };

                var httpResponse = await http.PostAsync(
                    _postToPagePhotosURL,
                    new FormUrlEncodedContent(postData)
                    );
                var httpContent = await httpResponse.Content.ReadAsStringAsync();

                return new Tuple<int, string>(
                    (int)httpResponse.StatusCode,
                    httpContent
                    );
            }
        }

        /// <summary>
        /// Update the uploaded picture (photo) with the given text
        /// </summary>
        /// <returns>StatusCode and JSON response</returns>
        /// <param name="postID">Post ID</param>
        /// <param name="postText">Text to add tp the post</param>
        public async Task<Tuple<int, string>> UpdatePhotoWithPost(string postID, string postText)
        {
            using (var http = new HttpClient())
            {
                var postData = new Dictionary<string, string> {
                { "access_token", _accessToken },
                { "message", postText }//,
                // { "formatting", "MARKDOWN" } // doesn't work
            };

                var httpResponse = await http.PostAsync(
                    $"{_facebookAPI}{postID}",
                    new FormUrlEncodedContent(postData)
                    );
                var httpContent = await httpResponse.Content.ReadAsStringAsync();

                return new Tuple<int, string>(
                    (int)httpResponse.StatusCode,
                    httpContent
                    );
            }
        }


        public async Task<Tuple<int, string>> PutPost(string idPost, string newPostText)
        {
            using (var http = new HttpClient())
            {
                var postData = new Dictionary<string, string>
                {
                    {"message", newPostText}
                };

                HttpResponseMessage httpResponse = await http.PostAsync(
                    $"{_facebookAPI}{idPost}?access_token={_accessToken}",
                    content: new FormUrlEncodedContent(postData));


                var httpContent = await httpResponse.Content.ReadAsStringAsync();

                return new Tuple<int, string>(
                    (int)httpResponse.StatusCode,
                    httpContent
                );

            }
        }

        public async Task<Tuple<int, string>> DeletePost(string idPost)
        {
            using (var http = new HttpClient())
            {

                HttpResponseMessage httpResponse = await http.DeleteAsync(
                    $"{_facebookAPI}{idPost}?access_token={_accessToken}");


                var httpContent = await httpResponse.Content.ReadAsStringAsync();

                return new Tuple<int, string>(
                    (int)httpResponse.StatusCode,
                    httpContent
                );

            }
        }



        /// <summary>
        /// Post picture with text from PC
        /// </summary>
        /// <param name="text">text for the post</param>
        /// <param name="imagePath">picture path for the post</param>
        public bool PostImageFromPC(string text, string imagePath)
        {
            FacebookClient fb = new FacebookClient(_accessToken);
            var imagStream = File.OpenRead(imagePath);
            dynamic res = fb.Post("/me/photos", new
            {
                message = text,
                file = new FacebookMediaStream
                {
                    ContentType = "image/jpg",
                    FileName = Path.GetFileName(imagePath)
                }.SetValue(imagStream)
            });
            return true;
        }
    }
}
