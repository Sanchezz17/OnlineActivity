import React, { Component } from 'react';

import styles from './chat.module.css';

export default class Chat extends Component {
    state = {
        loadingMessage: true,
        messages: [],
        currentMessage: {
            from: '',
            text: ''
        }
    };

    componentDidMount() {
        setInterval(this.fetchMessages, 500);
    }

    fetchMessages = () => {
        fetch(`/api/messages/${this.props.roomId}`)
            .then(response => response.json())
            .then(messages => this.setState({
                ...this.state,
                loadingMessage: false,
                messages
            }))
    };

    onClickEnter = () => {
        fetch(`/api/messages/${this.props.roomId}`, {
            body: JSON.stringify(this.state.currentMessage),
            headers: {'Content-Type': 'application/json'},
            method: 'POST'
        }).then(this.fetchMessages)
    };

    onChangeInput = (event) => {
        const { value } = event.target;

        this.setState({
            ...this.state,
            currentMessage: {
                from: this.props.user.name,
                text: value
            }
        });
    };

    onBlurInput = (event) => {
        event.target.value = '';
    };

    render() {
        return (
            <section className={styles.chat}>
                <header className={styles.chat__title}>Чат</header>
                {
                    this.state.loadingMessage ?
                        <p>Loading...</p> :
                        <section className={`chat__messages ${styles.messages}`}>
                            {this.state.messages.map(message =>
                                <p className={`messages__item ${styles.message}`}>
                                    <p className={styles.message__from}>{`${message.from}:`}</p>
                                    <p className={styles.message__text}>{message.text}</p>
                                </p>
                            )}
                        </section>
                }
                <input className={`chat__sendForm ${styles.form}`}
                       type='text' onChange={this.onChangeInput} onBlur={this.onBlurInput}/>
                <button className={styles.chat__button} onClick={this.onClickEnter}>Отправить</button>
            </section>
        )
    }
}
