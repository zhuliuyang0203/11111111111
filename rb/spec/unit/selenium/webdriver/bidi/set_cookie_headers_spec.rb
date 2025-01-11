# frozen_string_literal: true

# Licensed to the Software Freedom Conservancy (SFC) under one
# or more contributor license agreements.  See the NOTICE file
# distributed with this work for additional information
# regarding copyright ownership.  The SFC licenses this file
# to you under the Apache License, Version 2.0 (the
# "License"); you may not use this file except in compliance
# with the License.  You may obtain a copy of the License at
#
#   http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing,
# software distributed under the License is distributed on an
# "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
# KIND, either express or implied.  See the License for the
# specific language governing permissions and limitations
# under the License.

require File.expand_path('../spec_helper', __dir__)
require File.expand_path('../../../../../lib/selenium/webdriver/bidi/network/set_cookie_headers', __dir__)

module Selenium
  module WebDriver
    class BiDi
      describe SetCookieHeaders do
        let(:initial_hash) { {} }
        let(:cookie_headers) { described_class.new(initial_hash) }

        describe '#initialize' do
          it 'stores the passed set_cookie_headers hash internally' do
            my_hash = {'first_cookie' => {value: 'abc123'}}
            cookie_headers = described_class.new(my_hash)
            expect(cookie_headers.all).to eq(my_hash)
          end
        end

        describe '#all' do
          it 'returns the entire internal set_cookie_headers hash' do
            cookie_headers.add_set_cookie_header('cookie1', 'value1')
            expect(cookie_headers.all).to eq({'cookie1' => {value: 'value1'}})
          end
        end

        describe '#add_set_cookie_header' do
          it 'adds a cookie header to the internal store with a default hash' do
            cookie_headers.add_set_cookie_header('name', 'chocolate_chip')
            expect(cookie_headers['name']).to eq({value: 'chocolate_chip'})
          end

          it 'overwrites an existing cookie header if the name already exists' do
            cookie_headers.add_set_cookie_header('cookie_name', 'old_value')
            cookie_headers.add_set_cookie_header('cookie_name', 'new_value')
            expect(cookie_headers['cookie_name']).to eq({value: 'new_value'})
          end
        end

        describe '#remove_set_cookie_header' do
          it 'removes the named cookie header' do
            cookie_headers.add_set_cookie_header('session_id', 'abc123')
            cookie_headers.remove_set_cookie_header('session_id')
            expect(cookie_headers['session_id']).to be_nil
          end

          it 'does not raise an error if the cookie header does not exist' do
            expect { cookie_headers.remove_set_cookie_header('non_existent') }.not_to raise_error
          end
        end

        describe '#set_cookie_header' do
          it 'stores a cookie header with extended attributes' do
            cookie_headers.set_cookie_header(
              name: 'test_cookie',
              domain: 'example.com',
              path: '/',
              http_only: true,
              expiry: 1_700_000_000,
              max_age: 3600,
              same_site: 'None',
              secure: true
            )

            data = cookie_headers['test_cookie']
            expect(data[:domain]).to eq('example.com')
            expect(data[:path]).to eq('/')
            expect(data[:httpOnly]).to be(true)
            expect(data[:expiry]).to eq(1_700_000_000)
            expect(data[:maxAge]).to eq(3600)
            expect(data[:sameSite]).to eq('None')
            expect(data[:secure]).to be(true)
          end

          it 'uses "input" as the literal value for :value, per original code' do
            cookie_headers.set_cookie_header(name: 'my_cookie')
            expect(cookie_headers['my_cookie'][:value]).to eq('input')
          end
        end

        describe '#[]=' do
          it 'adds or updates a cookie header via bracket assignment' do
            cookie_headers['key1'] = 'value1'
            expect(cookie_headers['key1']).to eq({value: 'value1'})

            cookie_headers['key1'] = 'updated_value'
            expect(cookie_headers['key1']).to eq({value: 'updated_value'})
          end
        end

        describe '#[]' do
          it 'retrieves the cookie header hash by name' do
            cookie_headers['key2'] = 'value2'
            expect(cookie_headers['key2']).to eq({value: 'value2'})
          end

          it 'returns nil if the cookie header does not exist' do
            expect(cookie_headers['does_not_exist']).to be_nil
          end
        end

        describe '#delete' do
          it 'removes a cookie header via bracket-based delete' do
            cookie_headers['key3'] = 'value3'
            cookie_headers.delete('key3')
            expect(cookie_headers['key3']).to be_nil
          end
        end

        describe '#serialize' do
          context 'when set_cookie_headers is nil' do
            let(:initial_hash) { nil }

            it 'returns an empty array if no set_cookie_headers are provided' do
              expect(cookie_headers.serialize).to eq([])
            end
          end

          context 'when set_cookie_headers is a hash' do
            it 'returns an array of cookie header hashes with the specified format' do
              cookie_headers.set_cookie_header(
                name: 'test_cookie',
                domain: 'example.com',
                path: '/',
                http_only: true,
                expiry: 1_700_000_000
              )
              cookie_headers.add_set_cookie_header('second_cookie', 'second_value')

              serialized = cookie_headers.serialize
              expect(serialized).to be_an(Array)
              expect(serialized.size).to eq(2)

              # Check first cookie (extended fields)
              test_item = serialized.find { |h| h[:name] == 'test_cookie' }
              expect(test_item).not_to be_nil
              expect(test_item[:value]).to eq(type: 'string', value: 'input')
              expect(test_item[:domain]).to eq('example.com')
              expect(test_item[:path]).to eq('/')
              expect(test_item[:httpOnly]).to be(true)
              expect(test_item[:expiry]).to eq(1_700_000_000)

              # Check second cookie (just a simple string)
              second_item = serialized.find { |h| h[:name] == 'second_cookie' }
              expect(second_item).not_to be_nil
              expect(second_item[:value]).to eq(type: 'string', value: 'second_value')
              expect(second_item[:domain]).to be_nil
            end

            it 'returns an empty array if the internal hash is empty' do
              expect(cookie_headers.serialize).to eq([])
            end
          end
        end
      end
    end
  end
end
