using Notification.Core.Dto;
using Shared.Core.Dto;
using Shared.Core.Enum;

namespace Notification.Core.Template;

public class EmailTemplate
{
    public static EmailRequestDto BuildConfirmEmail(UserDto userDto, string code)
    {
        var subject = "MiniPay - Confirme seu E-mail";

        var confirmationUrl = $"https://localhost:5000/api/confirm-email/{code}";

        var body = $"""
                    Olá, {userDto.Name}!

                    Obrigado por se registrar no MiniPay. Para concluir seu cadastro, por favor confirme seu e-mail clicando no link abaixo:

                    {confirmationUrl}

                    Se você não solicitou este cadastro, ignore este e-mail.

                    Atenciosamente,  
                    Equipe MiniPay
                    """;

        return new EmailRequestDto(
            To: userDto.Email,
            Subject:  subject,
            Body: body
        );
    }

    public static EmailRequestDto BuildWelcomeEmail(UserDto userDto)
    {
        var subject = "Bem-vindo ao MiniPay 🎉";

        var body = $"""
                    Olá, {userDto.Name}!

                    Sua conta no MiniPay foi criada com sucesso e já está pronta para uso.

                    Agora você pode:
                    - Realizar transferências
                    - Consultar seu saldo
                    - Receber depósitos
                    - E muito mais!

                    Acesse o sistema e explore os recursos disponíveis.

                    Se tiver qualquer dúvida, entre em contato com nosso suporte.

                    Seja bem-vindo(a) e bons negócios!

                    Atenciosamente,  
                    Equipe MiniPay
                    """;

        return new EmailRequestDto(
            To: userDto.Email,
            Subject:  subject,
            Body: body
        );
    }
    
    public static EmailRequestDto BuildTransactionEmail(UserDto userDto, TransactionDto transactionDto, bool success)
    {
        var subject = success
            ? "✅ Transação concluída com sucesso"
            : "❌ Falha ao processar transação";

        var body = $"""
                    Olá!

                    Sua transação foi processada com os seguintes detalhes:

                    • Código: {transactionDto.Code}
                    • Valor: {transactionDto.Amount:C}
                    • Tipo: {transactionDto.Type}
                    • Status: {transactionDto.Status}
                    • Data: {transactionDto.CreatedAt:dd/MM/yyyy HH:mm}

                    {(success ? "Obrigado por usar nosso serviço!" : "Por favor, verifique sua carteira ou tente novamente.")}

                    Atenciosamente,
                    Equipe Financeira
                    """;

        return new EmailRequestDto(
            To: userDto.Email,
            Subject:  subject,
            Body: body
        );
    }
}