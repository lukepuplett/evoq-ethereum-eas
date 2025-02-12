# Ethereum Attestation Service (EAS)

EAS Explorer: https://easscan.org/schemas

At its core, an attestation is a digital signature on structured data. No matter where that data and signature are stored, if the data changes, the signature won't be valid.

The UID is usually* a hash of the whole attestation payload.

*provided the hash doesn't collide, in which case it "bumps" the payload, see EAS.sol

Two primary contracts:

 - Schema registry
 - Attestations

## Schema Record

```
/// @notice A struct representing a record for a submitted schema.
struct SchemaRecord {
    bytes32 uid; // The unique identifier of the schema.
    ISchemaResolver resolver; // Optional schema resolver.
    bool revocable; // Whether the schema allows revocations explicitly.
    string schema; // Custom specification of the schema (e.g., an ABI).
}
```

Schemas use the Solidity ABI for types and these are the basic ones:

 - `address` - An address can be any Ethereum address or contract address.
 - `string` - A string can be any text of arbitrary length
 - `bool` - A bool can either be true or false.
 - `bytes32` - A bytes32 is a 32 byte value. Useful for unique identifiers or small data.
 - `bytes` - A bytes value is an arbitrary byte value.
 - `uints` - uint values can be from uint8 -> uint256.
 - `<type>[]` - A variable-length array of elements of the given type (ex. address[]).

These are the data held in a schema record:

 - Schema # - this is an incremental number automatically assigned to the Schema. It is not a unique identifier.
 - UID - this is the unique universal identifier assigned to the schema.
 - Creator - the wallet address that created the schema.
 - Transaction ID - the Ethereum transaction registering the schema on EAS.
 - Resolver Contract - An optional contract assigned to the Schema for more complex use cases.
 - Attestation Count - The amount of attestations that have been made with attestations on/off chain.
 - Schema - The ABI encoded schema field types.

## Making and Registering Schemas

No code builder GUI: https://easscan.org/schema/create

JavaScript SDK: https://docs.attest.sh/docs/tutorials/create-a-schema

Example JS code: https://docs.attest.sh/docs/tutorials/create-a-schema#use-the-sdk

const schema = "uint256 eventId, uint8 voteIndex";

The schema definition string looks like the innards of an ABI function signature.

## Making an Attestation

 - Via the EAS SDK for JavaScript
 - Directly from contract to contract
 - Manually via the EASScan website

This is what's in an attestation record:

 - UID - this is a universal unique identification number for the attestation.
 - Schema - the UID of the schema used to make the attestation.
 - Attestor - the address that made the attestation.
 - Recipient - an optional recipient of the attestation that was made.
 - Expiration Time - an optional date that the attestation will expire if provided by the attestor.
 - Revocation Time - the time at which the attestation was revoked (if any).
 - refUID - An attestation that this attestation is referencing (if any).
 - data- The ABI encoded data for the attestation.
    
Note - Attestations can reference other attestations.

```
/// @notice A struct representing the arguments of the attestation request.
struct AttestationRequestData {
    address recipient; // The recipient of the attestation.
    uint64 expirationTime; // The time when the attestation expires (Unix timestamp).
    bool revocable; // Whether the attestation is revocable.
    bytes32 refUID; // The UID of the related attestation.
    bytes data; // Custom attestation data.
    uint256 value; // An explicit ETH amount to send to the resolver. This is important to prevent accidental user errors.
}
```


## On-chain vs Off-chain

It seems as though it's possible to create an attestation (signed data) according to the specification that is never held on-chain in whole or in part.

However, for verifiable timestamps, will need to timestamped on-chain and incur gas. 

 - On-chain attestations should minimize data, by attesting only to hashes over data held off-chain, maximise privacy by encrypting or salt-hashing sensitive data, and 
consider gas.

 - Off-chain ones should ensure secure storage, ensure storage redundancy by distributing copies or using IPFS, make for easy retrieval using a simple known protocol, use verifiable timestamps by submitting the UID on-chain.

## Off-chain & Merkle Proofs

It feels as though they really want you to use a Merkle tree for off-chain attestations and to just attest the proof. Then you share parts of the tree with whoever is asking for it.

The example on GitHub does not use a Merkle tree and instead uses a SchemaEncoder class which is in the EAS JS library.

It looks as though there are no Merkle Tree implementations for .NET and several ways to represent as JSON.

I think using the "nested object" JSON format will be the easiest to grok and an LLM can produce the code to create a tree.

Here is a .NET library with a Keccak-256 hasher and many other things:

https://www.bouncycastle.org/releasenotes.html

Privacy Ideas

 - Attest a Merkle* root, see https://brilliant.org/wiki/merkle-tree/ and https://docs.attest.sh/docs/tutorials/private-data-attestations#understanding-merkle-trees
 - Attest the result of some process, rather than the whole data
 - Make a ZK proof

A Merkle tree allows for selectively revealing parts of the data that make up the tree by sharing it's original data, and a Merkle proof which are a set of hashes that lead to the root.

EAS community has a web utility for making private data Merkle tree attestations and a preregistered "privateData" schema UID:

```
0x20351f973fdec1478924c89dfa533d8f872defa108d9c3c6512267d7e7e5dbc2
```

This web utility can generate the attestation, storing just the Merkle root in the privateData field. It also lets uses select the data values to reveal and generate a Merkle proof to send to someone else.

What's confusing that the docs show the web UI with the original data in cleartext, and a note saying that the "data is only visible to those who hold the proofs" but if the only thing on-chain is the root hash, then where did it get this cleartext data from?

## Resolver Contracts

A hook for a schema that can be used to validate data before it commits, or other custom actions.

E.g. could incentivize attestations by paying out a kickback to the attestor.

There's a sample contract in the EAS GitHub repo but otherwise the interface doesn't seem documented.

## Delegating

Separation of signer to transaction submitter (and payer). Docs mention a proxy idea but I don't understand it's benefits (to do with nonces and EIP-712) but they link to an example proxy contract:

https://github.com/ethereum-attestation-service/eas-contracts/blob/master/contracts/eip712/proxy/examples/PermissionedEIP712Proxy.sol

## Naming & Describing Schemas

You attest to the name after creating your schema by submitting an attestation using a special schema (schema #1 on most chains).

This schema has UID and Name fields and is revocable, so you can rename it. The UID is the ID of the schema to name. Do not add a recipient.

The EAS explorer will dereference the name is the creator of your named schema is also the attester/signer of the name attestation.

The same is true for descriptions which should use the Schema Description schema. Do not add a recipient.

## Schema Context

You can also attest to link your schema to a formal "context" such as an entity on schema.org

-

This somehow encodes an off-chain attestation into the URL fragment. The process by which this is done is not documented.

https://sepolia.easscan.org/offchain/url/#attestation=eNqlUUluHDEM%2FEufB4a4k0fPjOcTQQ5aqAcECeDnh93xBwLzUKBYVFEs%2FTjaG%2BpxAwCRgtvRPh%2BoFvv%2BJFvC5P5IDnrB825C2J5ImuAgeZzNBmHLaXVRcRudgQfAImqpgMQ2%2B5w5R99N2trZaCGLMUPiVJiXSJOYROI%2BZfnW3BO7V7JcNmxjcVzWVcB1dszsnEQ7JqyxEdyOG9qpk5nq%2FeORd36PjE3onXVSvGq2035vSDGfr2uodyluiw7aEKFWa3OjQSbcsgEL4xwUAqZdFbgFjVYedIux8N%2FLKTRGVbXPPSKwb%2BElgMZTpqsrw%2BxCzAm9z9UGbEne3vpsjrhPkXJfgwHYXG7tKvz%2B9ScvY74V%2BL3rjcuJWiGsvv0LQbNw6xlUXL38qoCxsYpRYeVVrW%2FEYi4FwWZULJqeWP1cfHVXjxnXWU7mUhqaus2uWViZXHwUwv9vcJSf8PMv1OWspA%3D%3D
