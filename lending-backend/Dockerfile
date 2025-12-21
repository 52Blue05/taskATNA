FROM golang:1.25

WORKDIR /app

COPY go.mod ./
RUN go mod download

COPY . .

CMD ["go", "run", "cmd/api/main.go"]
