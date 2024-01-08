alias ligo="docker run --rm -v "$PWD":"$PWD" --platform linux/amd64 -w "$PWD" ligolang/ligo:0.65.0"
export OCTEZ_CLIENT_UNSAFE_DISABLE_DISCLAIMER=yes
# octez-client --endpoint https://rpc.tzkt.io/jakartanet config update
# octez-client --endpoint https://rpc.ghostnet.teztnets.xyz/ config update
octez-client --endpoint https://ghostnet.tezos.marigold.dev/ config update
octez-client import secret key holder unencrypted:edsk3oRzLs4nUp4TrqsSJxqX9yMN1Jd6h2dx1SJf9DDWgr4tXbkRqm --force
octez-client get balance for holder
octez-client --wait none transfer 0 from holder to KT1DCcniV9tatQFVLnPv15i4kGYNgpdE6GhS --arg '4' --burn-cap '1.0'
ligo compile parameter '../main.jsligo' '{val1:"str1",val2:"str2"}'
octez-client --wait none transfer 0 from holder to KT1E4xgc9iniojkZqs1BDs117bzaYfMHZcPs --arg '(Pair "str1" "str2")' --burn-cap '1.0'
ligo compile storage 'incrementer.jsligo' '{inventories:Big_map.empty as big_map<address,inventory>, viewerContract:"KT1KvDaiC7sn6sdYyWncaYFUoivwHoK1pJM7" as address}'
octez-client originate contract MINTER transferring 0 from holder running incrementer.tz --init 'Pair {} "KT1KvDaiC7sn6sdYyWncaYFUoivwHoK1pJM7"' --burn-cap 1.0


octez-client gen keys holder
octez-client list known addresses
octez-client show address holder -S

ligo compile contract '../main.jsligo' > 'FA2.tz'
ligo compile storage '../main.jsligo' '{
    ledger: Big_map.empty as big_map<address,map<nat,nat>>,
    token_metadata: Big_map.empty as big_map<nat, {token_id : nat, token_info : map<string, bytes>}>,
    operators: Big_map.empty as big_map<[address, address], set<nat>>,
    marketplace: Big_map.empty as big_map<[address, nat], [nat, nat]>,
    token_counter:(1 as nat)
}'

octez-client originate contract FA2 transferring 0 from holder running FA2.tz --init '(Pair (Pair (Pair {} {}) {} 1) {})' --burn-cap 5.0


ligo compile parameter '../main.jsligo' 'SetMeta({token_id:0 as nat, token_info:Map.literal( list([
    ["item", Bytes.pack( {itemType:0, damage:0,armor:0,attackSpeed:0,healthPoints:0,manaPoints:0} )],

    ["name", Bytes.pack("Example Coin")],
    ["symbol", Bytes.pack("UnityTezos")],
    ["decimals", Bytes.pack(0)],

    ["image", Bytes.pack("ipfs://bafybeian23odhsho6gufacrcpcr65ft6bpqavzk36pt22lhcjoxy45mqpa")],
    ["artifactUri", Bytes.pack("ipfs://bafybeian23odhsho6gufacrcpcr65ft6bpqavzk36pt22lhcjoxy45mqpa")],
    ["displayUri", Bytes.pack("ipfs://bafybeian23odhsho6gufacrcpcr65ft6bpqavzk36pt22lhcjoxy45mqpa")],
    ["thumbnailUri", Bytes.pack("ipfs://bafybeian23odhsho6gufacrcpcr65ft6bpqavzk36pt22lhcjoxy45mqpa")],
    ["description", Bytes.pack("Unity Tezos Example Project coins used as soft currency")],
    ["minter", Bytes.pack(Tezos.get_sender())],
    ["creators", Bytes.pack(["https://assetstore.unity.com/packages/essentials/tutorial-projects/ui-toolkit-sample-dragon-crashers-231178"])],
    ["isBooleanAmount", Bytes.pack(false)],

    ["date", Bytes.pack(Tezos.get_now())]

    ]) )})'
    
// as one line:
ligo compile parameter '../main.jsligo' 'SetMeta({token_id:0 as nat, token_info:Map.literal( list([ ["item", Bytes.pack( {itemType:0, damage:0,armor:0,attackSpeed:0,healthPoints:0,manaPoints:0} )],  ["name", Bytes.pack("Example Coin")], ["symbol", Bytes.pack("UnityTezos")], ["decimals", Bytes.pack(0)],  ["image", Bytes.pack("ipfs://bafybeian23odhsho6gufacrcpcr65ft6bpqavzk36pt22lhcjoxy45mqpa")], ["artifactUri", Bytes.pack("ipfs://bafybeian23odhsho6gufacrcpcr65ft6bpqavzk36pt22lhcjoxy45mqpa")],  ["displayUri", Bytes.pack("ipfs://bafybeian23odhsho6gufacrcpcr65ft6bpqavzk36pt22lhcjoxy45mqpa")], ["thumbnailUri", Bytes.pack("ipfs://bafybeian23odhsho6gufacrcpcr65ft6bpqavzk36pt22lhcjoxy45mqpa")], ["description", Bytes.pack("Unity Tezos Example Project coins used as soft currency")], ["minter", Bytes.pack(Tezos.get_sender())], ["creators", Bytes.pack(["https://assetstore.unity.com/packages/essentials/tutorial-projects/ui-toolkit-sample-dragon-crashers-231178"])],  ["isBooleanAmount", Bytes.pack(false)], ["date", Bytes.pack(Tezos.get_now())]]) )})'

//result:
(Right
   (Right
      (Right
         (Right
            (Right
               (Right
                  (Right
                     (Right
                        (Left (Pair 0
                                    { Elt "artifactUri"
                                          0x050100000042697066733a2f2f62616679626569616e32336f646873686f3667756661637263706372363566743662707161767a6b3336707432326c68636a6f787934356d717061 ;
                                      Elt "creators"
                                          0x05010000006b68747470733a2f2f617373657473746f72652e756e6974792e636f6d2f7061636b616765732f657373656e7469616c732f7475746f7269616c2d70726f6a656374732f75692d746f6f6c6b69742d73616d706c652d647261676f6e2d63726173686572732d323331313738 ;
                                      Elt "date" 0x05003b ;
                                      Elt "decimals" 0x050000 ;
                                      Elt "description"
                                          0x050100000037556e6974792054657a6f73204578616d706c652050726f6a65637420636f696e73207573656420617320736f66742063757272656e6379 ;
                                      Elt "displayUri"
                                          0x050100000042697066733a2f2f62616679626569616e32336f646873686f3667756661637263706372363566743662707161767a6b3336707432326c68636a6f787934356d717061 ;
                                      Elt "image"
                                          0x050100000042697066733a2f2f62616679626569616e32336f646873686f3667756661637263706372363566743662707161767a6b3336707432326c68636a6f787934356d717061 ;
                                      Elt "isBooleanAmount" 0x050303 ;
                                      Elt "item" 0x0507070707070700000000070700000000070700000000 ;
                                      Elt "minter" 0x050a0000001600005af29a2dd5d55f9a2c41993dcf562317b20be264 ;
                                      Elt "name" 0x05010000000c4578616d706c6520436f696e ;
                                      Elt "symbol" 0x05010000000a556e69747954657a6f73 ;
                                      Elt "thumbnailUri"
                                          0x050100000042697066733a2f2f62616679626569616e32336f646873686f3667756661637263706372363566743662707161767a6b3336707432326c68636a6f787934356d717061 }))))))))))
