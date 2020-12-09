// Copyright 2020 Amazon.com, Inc. or its affiliates. All Rights Reserved.

// Handles trading an inventory item from one player to another player
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
    const playerID = event.requestContext.authorizer.claims.email;
    let errorMessage = '';
    let body = null; 
    if(typeof event.body == 'string')
    {
        body = JSON.parse(event.body);
    }
    else
    {
        body = event.body;
    }
    
    if(!body.hasOwnProperty('uid'))
    {
        errorMessage += 'Missing uid\n';
    }
    if(!body.hasOwnProperty('newowner_id'))
    {
        errorMessage += 'Missing newowner_id\n';
    }

    if(errorMessage !== '')
    {
        return {
            statusCode: 400,
            body: errorMessage
        };
    }

    // first, transfer the item to the new owner
    const sql = 
    `UPDATE inventory
     SET owner_id = '${body.newowner_id}'
     WHERE item_uid = ${body.uid}`;
    
    const sqlParams = {
        resourceArn: CONFIGURATION_DATA.inventoryDB_ARN,
        secretArn: CONFIGURATION_DATA.inventoryDBSecret_ARN,
        database: 'frogjunction_inventory',
        sql: sql
    };
    
    await rdsDataService.executeStatement(sqlParams).promise().then(data => {
        // no need to handle success, just move on
    }).catch(err => {
        errorMessage = 'newowner_id UPDATE failed: ' + err;
    });
    
    if(errorMessage !== '')
    {
        return {
            statusCode: 500,
            body: errorMessage
        };
    }    

    // second update the score
    // get the current score
    let params = {
      TableName: 'FrogJunctionPlayerData',
      Key: {'PlayerID' : playerID }
    };    

    let score = null;    
    await docClient.get(params).promise().then(data => {
        score = data.Item.score;
    }).catch(err => {
        errorMessage = 'get score failed: ' + err;
    });     


    // note that the item transfer will occur regardless if score can be updated
    // score can be fixed up on player login however
    if (errorMessage !== '')
    {
        return {
            statusCode: 500,
            body: errorMessage
        };
    }    

    // increment and write the score
    score++;

    params = {
      TableName: 'FrogJunctionPlayerData',
      Key: {'PlayerID' : playerID },
      UpdateExpression: "set score = :s",
      ExpressionAttributeValues: {":s":score}
    };    
    
    await docClient.update(params).promise().then(data => {
        // no need to handle success, just move on
    }).catch(err => {
        errorMessage = 'update score failed: ' +err;
    }); 
  
    if(errorMessage !== '')
    {
        return {
            statusCode: 500,
            body: errorMessage
        };
    }    
  
    // third grant a new item every 3 trades
    if(score % 3 == 0)
    {
        const arr_rand = function(arr) {return arr[Math.floor(Math.random() * arr.length)]};
        const newItem = arr_rand(ITEM_DESCS);
    
        sqlParams.sql =
        `INSERT INTO inventory(owner_id, type, in_pocket, wearable, worn, droppable, dropped, location_x, location_y)
         VALUES ('${playerID}', '${newItem.name}', TRUE, ${newItem.wearable}, FALSE, ${newItem.droppable}, FALSE, 0.0, 0.0)`;
    
    
        await rdsDataService.executeStatement(sqlParams).promise().then(data => {
            // no need to handle success, just move on
        }).catch(err => {
            errorMessage = 'grant new item failed: ' + err;
        });
        
        if(errorMessage !== '')
        {
            return {
                statusCode: 500,
                body: errorMessage
            };
        }    
    }

    return {
        statusCode: 200,
        body: 'Trade successful'
    }
} 