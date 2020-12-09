// Copyright 2020 Amazon.com, Inc. or its affiliates. All Rights Reserved.

// retrieve a players inventory from the database

const AWS = require('aws-sdk');
const docClient = new AWS.DynamoDB.DocumentClient();
const rdsDataService = new AWS.RDSDataService();
const CONFIGURATION_DATA = {
    inventoryDB_ARN: "arn:aws:rds:us-east-1:012345678901:cluster:frogjunction-inventory",
    inventoryDBSecret_ARN: "arn:aws:secretsmanager:us-east-1:012345678901:secret:frogjunction-inventory-database-1-a0aAAA",
};

exports.handler = async (event) => {
    let playerID = event.requestContext.authorizer.claims.email;
    const sql = `SELECT * FROM inventory WHERE owner_id = '${playerID}'`;

    const sqlParams = {
        resourceArn: CONFIGURATION_DATA.inventoryDB_ARN,
        secretArn: CONFIGURATION_DATA.inventoryDBSecret_ARN,
        database: 'frogjunction_inventory',
        sql: sql
    };

    let statusCode = null;
    
    await rdsDataService.executeStatement(sqlParams).promise().then(data => {
        // make the data a little easier to ingest in the client
        let inventory = [];
        for(const sqlitem of data.records) {
            let item = {};
            item.uid = sqlitem[0].longValue;
            item.name = sqlitem[2].stringValue;
            item.inpocket = sqlitem[3].booleanValue;
            item.wearable = sqlitem[4].booleanValue;
            item.worn = sqlitem[5].booleanValue;
            item.droppable = sqlitem[6].booleanValue;
            item.dropped = sqlitem[7].booleanValue;
            item.x = parseFloat(sqlitem[8].stringValue);
            item.y = parseFloat(sqlitem[9].stringValue);
            inventory.push(item);
        }
        // this is needed to be compatible with the Unity JSON API
        // there must be a top level object in order to decode it
        const items = {
            items: inventory
        }
        statusCode = {
            statusCode: 200,
            body: JSON.stringify(items)
        };
    }).catch(err => {
       statusCode = {
            statusCode: 500,
            body: JSON.stringify(err)
        };        
    });
    return statusCode;
};
