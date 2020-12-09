// Copyright 2020 Amazon.com, Inc. or its affiliates. All Rights Reserved.
const AWS = require('aws-sdk');
const docClient = new AWS.DynamoDB.DocumentClient();

// first iteration of the create player code before the inventory system
// has been developed

exports.handler = async (event) => {
    let errorMessage = '';
    let playerID = event.requestContext.authorizer.claims.email;
    let body = null; 
    if(typeof event.body == 'string')
    {
        body = JSON.parse(event.body);
    }
    else
    {
        body = event.body;
    }
    // verify the event has the following fields
    //  string name
    //  string model
    //  string house
    if(!body.hasOwnProperty('name'))
    {
        errorMessage += 'Missing name\n';
    }
    if(!body.hasOwnProperty('model'))
    {
        errorMessage += 'Missing model\n';
    }
    if(!body.hasOwnProperty('house'))
    {
        errorMessage += 'Missing house\n';
    }
    
    // This would be a good place to do additional validation
    // making sure the fields have data that makes sense, i.e.
    // valid model name(', etc. In a production game, rather
    // than have this data hard coded in the client, I would
    // have it stored in Amazon DynamoDB to access from both
    // client and service, and allow updating without needing
    // to release the client.
    
    if(errorMessage !== '')
    {
        return {
            statusCode: 400,
            body: errorMessage
        };
    }

    let putParams = {
        TableName: 'FrogJunctionPlayerData',
        Item: {
            'PlayerID' : playerID,
            'name' : body.name,
            'model' : body.model,
            'house' : body.house,
            'score' : 0
        }
    };

    let response;
    await docClient.put(putParams).promise().then(data => {
        response = {
            statusCode: 201,
            body: 'success, created data for playerID ' + playerID
        };
    }).catch(err => {
        response = {
            statusCode: 500,
            body: JSON.stringify(err)
        };
    });

    return response;
};
