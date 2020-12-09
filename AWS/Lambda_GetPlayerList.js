// Copyright 2020 Amazon.com, Inc. or its affiliates. All Rights Reserved.

// Retrieve the list of all registered players

const AWS = require('aws-sdk');
const docClient = new AWS.DynamoDB.DocumentClient();

exports.handler = async (event) => {

    var params = {
      TableName: 'FrogJunctionPlayerData',
    };

    
    let response = null;

// In practice we would paginate data, however that would
// complicate this demo, so this just shows a simple scan
    await docClient.scan(params).promise().then(data => {
      response = {
        statusCode: 200,
        body: JSON.stringify(data)
      };
    }).catch(err => {
      response = {
        statusCode: 500,
        body: JSON.stringify(err)
      };    
    }); 

    return response;
};
