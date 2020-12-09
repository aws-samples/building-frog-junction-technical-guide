// Copyright 2020 Amazon.com, Inc. or its affiliates. All Rights Reserved.
using Amazon;

//**** SANITIZE PRIOR TO PUBLIC RELEASE ****//
namespace FrogJunction
{
    // Typically these constants would be loaded from a configuration file, and that's a best practice
    // as you may have different AWS environments like test and prod so you'll have different endpoints.
    // Unity doesn't provide an appconfiguration type system, so you'd need to create your own. This
    // is out of the scope of the sample, so you can hard code your values here.
    class AWSStaticConfiguration
    {
        // modify these values based on the resource names provided by AWS
        public static readonly RegionEndpoint REGION = RegionEndpoint.USEast1;
        public const string USER_POOL_ID = "us-east-1_Ky42EjEZ2";
        public const string APP_CLIENT_ID = "50dfrqtf0s1aehcb1r8o9t9lbf";
        public const string API_ENDPOINT = "https://ybfshmoarc.execute-api.us-east-1.amazonaws.com/test";


        // modify these values only if you diverged from the names specified in the tutorial
        public const string PLAYER_DATA_TABLE = "FrogJunctionPlayerData";
        public const string PLAYER_DATA_PRIMARY_KEY = "PlayerID";
    }
}
