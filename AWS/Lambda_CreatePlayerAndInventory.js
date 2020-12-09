// Copyright 2020 Amazon.com, Inc. or its affiliates. All Rights Reserved.

// creates a new player and grants initial inventory

const AWS = require('aws-sdk');
const docClient = new AWS.DynamoDB.DocumentClient();
const rdsDataService = new AWS.RDSDataService();
const CONFIGURATION_DATA = {
    inventoryDB_ARN: "arn:aws:rds:us-east-1:012345678901:cluster:frogjunction-inventory",
    inventoryDBSecret_ARN: "arn:aws:secretsmanager:us-east-1:012345678901:secret:frogjunction-inventory-database-1-a0aAAA",
};

// Items are hard coded to simplify the demo. Ideally this info would come 
// from a shared configuration file or database.
const ITEM_DESCS = [
    {name: 'beanie', wearable: 'TRUE', droppable: 'FALSE'},
    {name: 'flowercrown', wearable: 'TRUE', droppable: 'FALSE'},
    {name: 'magehat', wearable: 'TRUE', droppable: 'FALSE'},
    {name: 'thimble', wearable: 'TRUE', droppable: 'FALSE'},
    {name: 'prop_pinwheel', wearable: 'FALSE', droppable: 'TRUE'},
    {name: 'prop_lawnset', wearable: 'FALSE', droppable: 'TRUE'},
    {name: 'prop_hammock', wearable: 'FALSE', droppable: 'TRUE'},
    {name: 'prop_flowerpot', wearable: 'FALSE', droppable: 'TRUE'},
];

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
    
    let statusCode = null;

    await docClient.put(putParams).promise().then(data => {
        // all is good, continue
    }).catch(err => {
        statusCode = {
            statusCode: 500,
            body: JSON.stringify(err)
        };
    });
    
    if(statusCode !== null )
    {
        return statusCode;
    }
    
    // select two random inventory items, one wearable, one not
    const arr_rand = function(arr) {return arr[Math.floor(Math.random() * arr.length)]};
    const wearable = arr_rand(ITEM_DESCS.filter((item) => {return item.wearable === 'TRUE'}));
    const droppable = arr_rand(ITEM_DESCS.filter((item) => {return item.droppable === 'TRUE'}));
    
    const sql = 
    `INSERT INTO inventory(owner_id, type, in_pocket, wearable, worn, droppable, dropped, location_x, location_y)
     VALUES ('${playerID}', '${wearable.name}', TRUE, ${wearable.wearable}, FALSE, FALSE, FALSE, 0.0, 0.0),
            ('${playerID}', '${droppable.name}', TRUE, FALSE, FALSE, ${droppable.droppable}, FALSE, 0.0, 0.0)`;
    
    const sqlParams = {
        resourceArn: CONFIGURATION_DATA.inventoryDB_ARN,
        secretArn: CONFIGURATION_DATA.inventoryDBSecret_ARN,
        database: 'frogjunction_inventory',
        sql: sql
    }
    
    await rdsDataService.executeStatement(sqlParams).promise().then(data => {
        statusCode = {
            statusCode: 201,
            body: 'success, created data for playerID ' + playerID
        };
    }).catch(err => {
       statusCode = {
            statusCode: 500,
            body: JSON.stringify(err)
        };        
    });

    return statusCode;
};
