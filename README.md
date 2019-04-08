h1. Simple Inventory API This is a simple API

*Version:* 1.0.0

----

{toc:printable=true|style=square|minLevel=2|maxLevel=3|type=list|outline=false|include=.*}

h2. Endpoints

    h3. addInventory
    {status:colour=Yellow|title=post|subtle=false}
    {code}
    post /inventory
    {code}
    *Summary:* adds an inventory item
    *Description:* Adds an item to the system


    h4. Parameters

        h5. Body Parameter
        ||Name||Description||Required||Default||Pattern||
        |inventoryItem |Inventory item to add |(x) | |  |







    h4. Responses
        *Status Code:* 201
        *Message:*     item created
        {code:title=Response Type}

        {code}
        See [#models]



        {code:title=Response Schema |collapse=true}
{
  "description" : "item created"
}
        {code}
        *Status Code:* 400
        *Message:*     invalid input, object invalid
        {code:title=Response Type}

        {code}
        See [#models]



        {code:title=Response Schema |collapse=true}
{
  "description" : "invalid input, object invalid"
}
        {code}
        *Status Code:* 409
        *Message:*     an existing item already exists
        {code:title=Response Type}

        {code}
        See [#models]



        {code:title=Response Schema |collapse=true}
{
  "description" : "an existing item already exists"
}
        {code}
    ----

    h3. searchInventory
    {status:colour=Yellow|title=get|subtle=false}
    {code}
    get /inventory
    {code}
    *Summary:* searches inventory
    *Description:* By passing in the appropriate options, you can search for available inventory in the system 


    h4. Parameters



        h5. Query Parameters
        ||Name||Description||Required||Default||Pattern||
        |searchString |pass an optional search string for looking up inventory |(x) | |  |

|skip |number of records to skip for pagination |(x) | |  |

|limit |maximum number of records to return |(x) | |  |





    h4. Responses
        *Status Code:* 200
        *Message:*     search results matching criteria
        {code:title=Response Type}
array[InventoryItem]
        {code}
        See [#models]



        {code:title=Response Schema |collapse=true}
{
  "description" : "search results matching criteria",
  "schema" : {
    "type" : "array",
    "items" : {
      "$ref" : "#/definitions/InventoryItem"
    }
  }
}
        {code}
        *Status Code:* 400
        *Message:*     bad input parameter
        {code:title=Response Type}

        {code}
        See [#models]



        {code:title=Response Schema |collapse=true}
{
  "description" : "bad input parameter"
}
        {code}
    ----

h2. Models

        h3. InventoryItem
        ||Field Name||Required||Type||Description||
         |id | |UUID | |
 |name | |String | |
 |releaseDate | |Date | |
 |manufacturer | |Manufacturer | |
        h3. Manufacturer
        ||Field Name||Required||Type||Description||
         |name | |String | |
 |homePage | |String | |
 |phone | |String | |
