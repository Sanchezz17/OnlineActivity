import React, {Component} from 'react';
import * as signalR from '@aspnet/signalr';
import styles from './chat.module.css';

export default class Chat extends Component {
    SEND = 'send';
    NEW_MESSAGE = 'newMessage';
    JOIN_ROOM_CHAT = 'joinRoomChat';
    NOTIFY = 'notify';
    
    constructor(props) {
        super(props);
        this.state = {
            messages: [],
            currentMessage: '',
            hubConnection: null
        };
    }

    componentDidMount() {
        const hubConnection = new signalR.HubConnectionBuilder()
            .withUrl("/chat")
            .build();

        this.setState({ hubConnection: hubConnection }, async () => {
            await this.state.hubConnection.start()
                .then(() => console.log('Connection started!'))
                .catch(err => console.log('Error while establishing connection :('));
            
            this.state.hubConnection.on(this.NEW_MESSAGE, (from, text) => {
                this.setState({
                    messages: [{from, text}, ...this.state.messages]
                })
            });

            this.state.hubConnection.on(this.NOTIFY, (notification) => {
                this.setState({
                    messages: [{from: notification, text: ''}, ...this.state.messages]
                })
            });
            
            await this.state.hubConnection.invoke(this.JOIN_ROOM_CHAT, this.props.roomId, this.props.user.name);
        });

    }

    onPostMessage = (event) => {
        event.preventDefault();
        this.state.hubConnection.invoke(this.SEND, this.props.roomId, this.props.user.name, this.state.currentMessage);
        this.setState({currentMessage: ''})
    };

    onChangeInput = (event) => {
        const {value} = event.target;

        this.setState({
            ...this.state,
            currentMessage: value
        });
    };

    render() {
        return (
            <section className={styles.chat}>
                <header className={`btn-sm ${styles.title}`}>Чат</header>
                <section className={styles.messages}>
                    {this.state.messages.map(message =>
                        <p className={styles.message}>
                            <span className={styles.from}>{message.from}</span> <span className={styles.text}>{message.text}</span>
                        </p>
                    )}
                </section>
                <form className={styles.form} onSubmit={this.onPostMessage}>
                    <input
                        className={styles.input}
                        value={this.state.currentMessage}
                        placeholder="Отвечать тут..."
                        type='text'
                        onChange={this.onChangeInput}
                    />
                </form>

            </section>
        )
    }
}
