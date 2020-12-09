// Copyright 2020 Amazon.com, Inc. or its affiliates. All Rights Reserved.

// retrieve player data from the database

const AWS = require('aws-sdk');
const docClient = new AWS.DynamoDB.DocumentClient();

exports.handler = async (event) => {
    let playerID = event.requestContext.authorizer.claims.email;

    var params = {
      TableName: 'FrogJunctionPlayerData',
      Key: {'PlayerID' : playerID }
    };

    
    let response;
    
    await docClient.get(params).promise().then(data => {
      response = {
        statusCode: 200,
        body: JSON.stringify(data.Item)
      };
    }).catch(err => {
      response = {
        statusCode: 500,
        body: JSON.stringify(err)
      };    
    }); 

    return response;
};
