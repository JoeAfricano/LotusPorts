using System.Buffers.Binary;
using System.Text.RegularExpressions;
using Lotus.Integration.Contracts;

namespace LotusPorts.Integration.Tests;

public partial class ContractTests
{
    [Fact]
    public async Task Request_round_trips_through_framing()
    {
        var sent = JsonRpcMessage.Request("1", Methods.Handshake, new HandshakeParams("Lotus Ports", "0.1.0", Protocol.Version, ["ports.launch"]));
        using var stream = new MemoryStream();
        await MessageFraming.WriteAsync(stream, sent);
        stream.Position = 0;

        var received = await MessageFraming.ReadAsync(stream);

        received!.IsRequest.Should().BeTrue();
        received.Method.Should().Be(Methods.Handshake);
        received.ParamsAs<HandshakeParams>().Should().BeEquivalentTo(new HandshakeParams("Lotus Ports", "0.1.0", 1, ["ports.launch"]));
        (await MessageFraming.ReadAsync(stream)).Should().BeNull("the stream ended cleanly");
    }

    [Fact]
    public async Task Notification_and_error_kinds_are_told_apart()
    {
        var note = await RoundTrip(JsonRpcMessage.Notification(Events.DriverChanged, new { version = "x" }));
        note.IsNotification.Should().BeTrue();

        var error = await RoundTrip(JsonRpcMessage.Failure("9", JsonRpcErrorCodes.MethodNotFound, "nope"));
        error.IsResponse.Should().BeTrue();
        error.Error.Should().Be(new JsonRpcError(-32601, "nope"));
    }

    [Fact]
    public void Enums_are_serialized_as_text()
    {
        var message = JsonRpcMessage.Success("1", new { state = CheckStatus.NotVerified });
        message.Result!.Value.GetProperty("state").GetString().Should().Be("NotVerified");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(Protocol.MaxMessageBytes + 1)]
    public async Task Invalid_lengths_are_rejected(int length)
    {
        var header = new byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(header, length);
        var act = () => MessageFraming.ReadAsync(new MemoryStream(header));
        await act.Should().ThrowAsync<MessageFramingException>();
    }

    [Fact]
    public async Task Truncated_and_malformed_messages_are_rejected()
    {
        var truncated = () => MessageFraming.ReadAsync(new MemoryStream([10, 0, 0, 0, 1, 2]));
        await truncated.Should().ThrowAsync<MessageFramingException>();

        var body = "{not json"u8.ToArray();
        var framed = new byte[4 + body.Length];
        BinaryPrimitives.WriteInt32LittleEndian(framed, body.Length);
        body.CopyTo(framed, 4);
        var malformed = () => MessageFraming.ReadAsync(new MemoryStream(framed));
        await malformed.Should().ThrowAsync<MessageFramingException>();
    }

    [Fact]
    public void Dependency_ids_are_unique_kebab_case() =>
        DependencyIds.All.Should().OnlyHaveUniqueItems().And.AllSatisfy(id => KebabCase().IsMatch(id).Should().BeTrue(id));

    private static async Task<JsonRpcMessage> RoundTrip(JsonRpcMessage message)
    {
        using var stream = new MemoryStream();
        await MessageFraming.WriteAsync(stream, message);
        stream.Position = 0;
        return (await MessageFraming.ReadAsync(stream))!;
    }

    [GeneratedRegex("^[a-z0-9]+(-[a-z0-9]+)*$")]
    private static partial Regex KebabCase();
}
