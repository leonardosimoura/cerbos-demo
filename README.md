# cerbos Demo

Simple demo using [cerbos](https://github.com/cerbos/cerbos)

Need to pass manager role to create agreement.

To modify you need to be same user of agreement.

# Setup
## cerbos
docker run --rm --name cerbos -d -v $(pwd)/cerbos/policies:/policies -p 3592:3592 -p 3593:3593  ghcr.io/cerbos/cerbos:0.25.0