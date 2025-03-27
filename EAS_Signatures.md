# EAS Contract Functions

## Attestation Functions
```solidity
attest(AttestationRequest request) external payable returns (bytes32)
attestByDelegation(DelegatedAttestationRequest delegatedRequest) external payable returns (bytes32)
multiAttest(MultiAttestationRequest[] multiRequests) external payable returns (bytes32[])
multiAttestByDelegation(MultiDelegatedAttestationRequest[] multiDelegatedRequests) external payable returns (bytes32[])
```

## Revocation Functions
```solidity
revoke(RevocationRequest request) external payable
revokeByDelegation(DelegatedRevocationRequest delegatedRequest) external payable
revokeOffchain(bytes32 data) external returns (uint64)
multiRevoke(MultiRevocationRequest[] multiRequests) external payable
multiRevokeByDelegation(MultiDelegatedRevocationRequest[] multiDelegatedRequests) external payable
multiRevokeOffchain(bytes32[] data) external returns (uint64)
```

## Timestamp Functions
```solidity
timestamp(bytes32 data) external returns (uint64)
multiTimestamp(bytes32[] data) external returns (uint64)
```

## View/Query Functions
```solidity
getAttestation(bytes32 uid) external view returns (Attestation)
isAttestationValid(bytes32 uid) external view returns (bool)
getRevokeOffchain(address revoker, bytes32 data) external view returns (uint64)
getTimestamp(bytes32 data) external view returns (uint64)
getSchemaRegistry() external view returns (address)
getName() external view returns (string)
getVersion() external view returns (string)
getNonce(address account) external view returns (uint256)
getDomainSeparator() external view returns (bytes32)
getAttestTypeHash() external pure returns (bytes32)
getRevokeTypeHash() external pure returns (bytes32)
```

## EIP-712 Related
```solidity
eip712Domain() external view returns (bytes1, string, string, uint256, address, bytes32, uint256[])
increaseNonce(uint256 newNonce) external
```

## Key Notes
1. Most write operations have both regular and delegated versions
2. Many functions have multi-version variants for batch operations
3. All attestation and revocation functions are payable
4. There are comprehensive view functions for querying attestation state
5. The contract implements EIP-712 for structured data signing