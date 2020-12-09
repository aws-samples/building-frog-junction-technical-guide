// Copyright 2020 Amazon.com, Inc. or its affiliates. All Rights Reserved.

// persist the change of state for an inventory item

const AWS = require('aws-sdk');
const docClient = new AWS.DynamoDB.DocumentClient();
const rdsDataService = new AWS.RDSDataService();
const CONFIGURATION_DATA = {
    inventoryDB_ARN: "arn:aws:rds:us-east-1:012345678901:cluster:frogjunction-inventory",
    inventoryDBSecret_ARN: "arn:aws:secretsmanager:us-east-1:012345678901:secret:frogjunction-inventory-database-1-a0aAAA",
};


exports.handler = async (event) => {
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
    
    // initial parameter validation
    if(!body.hasOwnProperty('uid'))
    {
        return {
          statusCode: 400,
          body: "Missing uid"
        };
    }

    // get the current item from the db
    const sql = `SELECT * FROM inventory WHERE item_uid = ${body.uid}`;

    const sqlParams = {
        resourceArn: CONFIGURATION_DATA.inventoryDB_ARN,
        secretArn: CONFIGURATION_DATA.inventoryDBSecret_ARN,
        database: 'frogjunction_inventory',
        sql: sql
    };

    let response = null;
    let item = {};
    
    await rdsDataService.executeStatement(sqlParams).promise().then(data => {
        // parse data
        const sqlitem = data.records[0];    // there is only one item guaranteed by the DB
        item.uid = sqlitem[0].longValue;
        item.name = sqlitem[2].stringValue;
        item.inpocket = sqlitem[3].booleanValue;
        item.wearable = sqlitem[4].booleanValue;
        item.worn = sqlitem[5].booleanValue;
        item.droppable = sqlitem[6].booleanValue;
        item.dropped = sqlitem[7].booleanValue;
        item.x = parseFloat(sqlitem[8].stringValue);
        item.y = parseFloat(sqlitem[9].stringValue);
    }).catch(err => {
       response = {
            statusCode: 500,
            body: JSON.stringify(err)
        };        
    });    
    
    if(response !== null)
    {
        return response;
    }
    
    let modifysql = null;
    
    // determine what to do with the item
    if(item.worn)
    {
        // if item is worn, put it back in pocket
        modifysql = 
        `UPDATE inventory
         SET in_pocket = TRUE, worn = FALSE
         WHERE item_uid = ${body.uid}`;
    }
    else if(item.dropped)
    {
        // if item is dropped, put it back in pocket
        modifysql = 
        `UPDATE inventory
         SET in_pocket = TRUE, dropped = FALSE
         WHERE item_uid = ${body.uid}`;
    }
    else if(item.inpocket)
    {
        if(item.wearable)
        {
            // if item is wearable and in pocket, put it on
            modifysql = 
            `UPDATE inventory
             SET in_pocket = FALSE, worn = TRUE
             WHERE item_uid = ${body.uid}`;
        }
        else if(item.droppable)
        {
            // If item is droppable and in pocket, put it in the world.
            // Make sure coordinates have been set first
            if(!body.hasOwnProperty('x'))
            {
                   errorMessage += 'Dropped item missing x\n';     
            }            
            if(!body.hasOwnProperty('ypos'))
            {
                   errorMessage += 'Dropped item missing y\n';     
            }
            if(errorMessage !== '') {
                return {
                  statusCode: 400,
                  body: errorMessage
                };                
            }
            modifysql = 
            `UPDATE inventory
             SET in_pocket = FALSE, dropped = TRUE, location_x = ${body.x}, location_y = ${body.y}
             WHERE item_uid = ${body.uid}`;
        }
        else
        {
            // something is wrong, this should be impossible
            return {
              statusCode: 500,
              body: "Item is neither dropbbalbe or wearable, inventory item is set up incorrectly\n"
            };         
        }
    }
    else
    {
        return {
          statusCode: 500,
          body: "Item is not worn, dropped or inpocket, something is terribly wrong\n"
        };         
    }
    
    sqlParams.sql = modifysql;
    
    
    // everything is figured out at this point, execute the update query
    await rdsDataService.executeStatement(sqlParams).promise().then(data => {
        response = {
            statusCode: 200,
            body: 'success, item state changed'
        };
    }).catch(err => {
       response = {
            statusCode: 500,
            body: JSON.stringify(err)
        };        
    });    
    
    return response;
};
