<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="OpenAuthProviders.ascx.cs" Inherits="WebApplication1.Account.OpenAuthProviders" %>

<div id="socialLoginList">
    <h4>別のサービスを使用してログインします。</h4>
    <hr />
    <asp:ListView runat="server" ID="providerDetails" ItemType="System.String"
        SelectMethod="GetProviderNames" ViewStateMode="Disabled">
        <ItemTemplate>
            <p>
                <button type="submit" class="btn btn-default" name="provider" value="<%#: Item %>"
                    title="アカウントを使用して <%#: Item %> ログインします。">
                    <%#: Item %>
                </button>
            </p>
        </ItemTemplate>
        <EmptyDataTemplate>
            <div>
                <p>構成されている外部認証サービスがありません。外部サービスを介してログインをサポートするようこの ASP.NET アプリケーションを設定する方法の詳細については、<a href="http://go.microsoft.com/fwlink/?LinkId=252803">この資料</a>を参照してください。</p>
            </div>
        </EmptyDataTemplate>
    </asp:ListView>
</div>
